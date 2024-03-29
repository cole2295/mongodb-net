﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MongoDB.Driver.Command;
using MongoDB.Driver.Conditions;

namespace MongoDB.Driver
{
    internal class DBCollection : IDBCollection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DBCollection"/> class.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="collectionUri">The collection URI.</param>
        public DBCollection(Database database, Uri collectionUri)
        {
            Uri relative = collectionUri.IsAbsoluteUri ? collectionUri.MakeRelativeUri(database.Uri) : collectionUri;
            Uri = new Uri(new Uri(database.Uri.ToString() + "/"), relative);
            _Database = database;
        }

        /// <summary>
        /// Gets a value indicating whether the collection is read only.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the collection is read only; otherwise, <c>false</c>.
        /// </value>
        public bool ReadOnly
        {
            get
            {
                return Database.ReadOnly;
            }
        }

        /// <summary>
        /// Saves a series of documents to the database.
        /// </summary>
        /// <param name="documents">The series</param>
        /// <param name="checkError"></param>
        public void Insert(IEnumerable<IDocument> documents, bool checkError = false)
        {
            if (ReadOnly)
                throw new ReadOnlyException("Cannot insert documents when using a readonly binding");

            foreach (IDocument document in documents)
            {
                document.Collection = this;
            }
            Message.Insert insert = new Message.Insert(FullName, documents);
            _Database.Binding.Say(Database.CmdCollection, insert, checkError);
        }

        /// <summary>
        /// Tries to saves a series of documents to the database.
        /// </summary>
        /// <param name="documents">The series</param>
        /// <param name="checkError"></param>
        /// <returns></returns>
        public bool TryInsert(IEnumerable<IDocument> documents, bool checkError = false)
        {
            if (ReadOnly)
                return false;

            foreach (IDocument document in documents)
            {
                document.Collection = this;
            }
            Message.Insert insert = new Message.Insert(FullName, documents);
            return _Database.Binding.TrySay(Database.CmdCollection, insert, checkError);
        }

        /// <summary>
        /// Removes the specified o.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <param name="checkError">if set to <c>true</c> [check error].</param>
        public void Remove(IDocument o, bool checkError = false)
        {
            if (ReadOnly)
                throw new ReadOnlyException("Cannot remove document(s) when using a readonly binding");

            Message.Delete delete = new Message.Delete(o, Uri.ToString());
            _Database.Binding.Say(Database.CmdCollection, delete, checkError);
        }

        /// <summary>
        /// Tries the remove.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <param name="checkError">if set to <c>true</c> [check error].</param>
        /// <returns></returns>
        public bool TryRemove(IDocument o, bool checkError = false)
        {
            if (ReadOnly)
                return false;

            Message.Delete delete = new Message.Delete(o, Uri.ToString());
            return _Database.Binding.TrySay(Database.CmdCollection, delete, checkError);
        }

        /// <summary>
        /// Uses OP_QUERY message to retrieve the first batch of documents in a query
        /// </summary>
        /// <typeparam name="TDoc">A type that implements <see cref="T:MongoDB.Driver.IDocument"/>.</typeparam>
        /// <param name="cursor">The cursor.</param>
        /// <returns>The documents of this batch</returns>
        public IEnumerable<TDoc> Query<TDoc>(IDBCursor<TDoc> cursor) where TDoc : class, IDocument
        {
            Condition.Requires(cursor, "cursor").IsNotNull();
            Condition.Requires(cursor.Options.Collection.Name, "options.Collection.Name").IsEqualTo(this.Name, "options.Collection must be set to this collection");

            DBCursor.CleanupCursors(this);

            Message.Query query = new Message.Query(cursor.Options);

            IDBResponse response = _Database.Binding.Call<TDoc>(Database.CmdCollection, query);
            MongoDB.Driver.Message.Reply<TDoc> reply = response as MongoDB.Driver.Message.Reply<TDoc>;
            if (reply != null)
                cursor.CursorID = reply.CursorID;

            string error = response.GetError();
            if (!string.IsNullOrEmpty(error))
            {
                throw new Exception(string.Format("db error [{0}]", error));
            }
            return response.Documents.Cast<TDoc>();
        }

        /// <summary>
        /// Uses OP_GETMORE to retrieve more documents for the specified cursor
        /// </summary>
        /// <typeparam name="TDoc">A type that implements <see cref="T:MongoDB.Driver.IDocument"/>.</typeparam>
        /// <param name="cursor">The cursor.</param>
        /// <returns>All documents returned in the message</returns>
        public IEnumerable<TDoc> GetMore<TDoc>(IDBCursor<TDoc> cursor) where TDoc : class, IDocument
        {
            Message.GetMore getMore = new Message.GetMore(cursor);

            IDBResponse response = _Database.Binding.Call<TDoc>(Database.CmdCollection, getMore);
            MongoDB.Driver.Message.Reply<TDoc> reply = response as MongoDB.Driver.Message.Reply<TDoc>;
            if (reply != null)
                cursor.CursorID = reply.CursorID;

            string error = response.GetError();
            if (!string.IsNullOrEmpty(error))
            {
                throw new Exception(string.Format("db error [{0}]", error));
            }
            return response.Documents.Cast<TDoc>();
        }

        /// <summary>
        /// Performs an update operation.
        /// </summary>
        /// <param name="selector">search query for old object to update.</param>
        /// <param name="document">The document.</param>
        /// <param name="modifier">The modifier.</param>
        /// <param name="upsert">if set to <c>true</c> then the matching documents will either be updated or inserted (depending on existence)</param>
        /// <param name="multi">if set to <c>true</c> then allow the update of multiple matching documents.</param>
        /// <param name="checkError"></param>
        public void Update(DBQuery selector = null, IDocument document = null, DBModifier modifier = null, bool upsert = false, bool multi = false, bool checkError = false)
        {
            if (ReadOnly)
                throw new ReadOnlyException("Cannot update documents when using a readonly binding");

            selector = selector ?? DBQuery.SelectAll;
            Condition.Requires(document, "document")
                .Evaluate(document != null || modifier != null, "You must specify either document or modifier")
                .Evaluate(!(document == null && upsert), "You cannot upsert using a modifier expression");

            UpdateOption options = UpdateOption.None;
            if (upsert)
                options |= UpdateOption.Upsert;
            if (multi)
                options |= UpdateOption.MultiUpdate;

            Message.Update update = new Message.Update(this.FullName, selector, (IDBObject)document ?? (IDBObject)modifier, options);
            _Database.Binding.Say(Database.CmdCollection, update, checkError);
        }

        /// <summary>
        /// Tries to perform an update operation.
        /// </summary>
        /// <param name="selector">search query for old object to update.</param>
        /// <param name="document">The document.</param>
        /// <param name="modifier">The modifier.</param>
        /// <param name="upsert">if set to <c>true</c> then the matching documents will either be updated or inserted (depending on existence)</param>
        /// <param name="multi">if set to <c>true</c> then allow the update of multiple matching documents.</param>
        /// <param name="checkError"></param>
        /// <returns></returns>
        public bool TryUpdate(DBQuery selector = null, IDocument document = null, DBModifier modifier = null, bool upsert = false, bool multi = false, bool checkError = false)
        {
            if (ReadOnly)
                throw new ReadOnlyException("Cannot update documents when using a readonly binding");

            selector = selector ?? DBQuery.SelectAll;
            Condition.Requires(document, "document")
                .Evaluate(document != null || modifier != null, "You must specify either document or modifier")
                .Evaluate(!(document == null && upsert), "You cannot upsert using a modifier expression");

            UpdateOption options = UpdateOption.None;
            if (upsert)
                options |= UpdateOption.Upsert;
            if (multi)
                options |= UpdateOption.MultiUpdate;

            Message.Update update = new Message.Update(this.FullName, selector, (IDBObject)document ?? (IDBObject)modifier, options);
            return _Database.Binding.TrySay(Database.CmdCollection, update, checkError);
        }

        /// <summary>
        /// Ensures the index.
        /// </summary>
        /// <param name="indexKeyFieldSet">The index key field set.</param>
        /// <param name="indexUri">Name of the index.</param>
        /// <param name="unique">if set to <c>true</c> [unique].</param>
        /// <returns></returns>
        public IDBIndex EnsureIndex(DBFieldSet indexKeyFieldSet, Uri indexUri, bool unique)
        {
            Condition.Requires(indexKeyFieldSet, "indexKeyFieldSet").IsNotNull().IsNotEmpty();

            if (indexUri == null)
                indexUri = indexKeyFieldSet.GenerateIndexUri();

            DBIndex index = null;
            if (!_IndexLookup.TryGetValue(indexUri.GetIndexName(), out index))
            {
                index = new DBIndex(this, indexUri, indexKeyFieldSet, unique);
                _EnsureIndex(index);
            }
            return index;
        }

        void _EnsureIndex(DBIndex index)
        {
            if (ReadOnly)
                throw new ReadOnlyException("Cannot add an index while using a readonly binding");
            IDocument o = new Document() 
            {
                {"name", index.Name},
                {"ns", this.FullName},
                {"key", index.KeyFieldSet},
                {"unique", index.Unique},
            };
            _Database.GetCollection("system.indexes").Insert(o);
        }

        /// <summary>
        /// Returns a hash code for this instance. (based off of the full name of the collection)
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return Uri.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">The <paramref name="obj"/> parameter is null.</exception>
        public override bool Equals(object obj)
        {
            IDBCollection col = obj as IDBCollection;
            if (obj != null)
                return Uri.GetFullCollectionName().Equals(col.Uri.GetFullCollectionName());
            return false;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance. (based off of the collection's local name)
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Uri.GetCollectionName();
        }

        /// <summary>
        /// Ensures an index on the id field, if one does not already exist.
        /// </summary>
        /// <param name="obj">an object with an _id field.</param>
        public void ConditionallyEnsureIDIndex(IDBObject obj)
        {
            if (_IdIndex != null) // we already created it, so who cares
                return;

            if (obj["_id"] == null)
                return;

            if (obj.Keys.Count > 1)
                return;

            EnsureIDIndex();
        }

        /// <summary>
        /// Creates an index on the id field, if one does not already exist.
        /// </summary>
        /// <returns></returns>
        public IDBIndex EnsureIDIndex()
        {
            if (_IdIndex != null)
                return _IdIndex;
            return _IdIndex = this.EnsureIndex(DBFieldSet.IDKeyFieldSet, "_id", true);
        }

        /// <summary>
        /// Clears all indices that have not yet been applied to this collection.
        /// </summary>
        public void ClearIndexCache()
        {
            _IndexLookup.Clear();
        }

        internal Database _Database { get; private set; }
        /// <summary>
        /// Gets the DB.
        /// </summary>
        /// <value>The DB.</value>
        public IDatabase Database { get { return _Database; } }

        /// <summary>
        /// Gets the absolute URI for this collection.
        /// </summary>
        /// <value>The URI.</value>
        public Uri Uri { get; protected set; }

        List<DBFieldSet> __hintFields = new List<DBFieldSet>();
        /// <summary>
        /// Gets or sets the index hint field sets.
        /// </summary>
        /// <value>The index hint field sets.</value>
        public IEnumerable<DBFieldSet> IndexHintFieldSets { get { return __hintFields; } set { __hintFields = new List<DBFieldSet>(value); } }

        IDBIndex _IdIndex = null;


        SortedDictionary<string, DBIndex> _IndexLookup = new SortedDictionary<string, DBIndex>();


        /// <summary>
        /// Enumerates all indexes known to the server.
        /// </summary>
        /// <value>The indexes.</value>
        public IEnumerable<IDBIndex> Indexes
        {
            get
            {
                foreach (IDBObject infoObject in this.GetIndexInfo())
                {
                    Uri indexUri = new Uri(infoObject.GetAsString("name"), UriKind.RelativeOrAbsolute);

                    DBIndex index = null;
                    if (!_IndexLookup.TryGetValue(indexUri.GetIndexName(), out index))
                    {
                        DBFieldSet fieldSet = new DBFieldSet(infoObject.GetAsIDBObject("key").Keys);
                        bool unique = infoObject.ContainsKey("unique");
                        index = new DBIndex(this, indexUri, fieldSet, unique);
                        _IndexLookup.Add(indexUri.GetIndexName(), index);
                    }
                    yield return index;
                }
            }
        }

        /// <summary>
        /// Gets the index specified by the URI.
        /// </summary>
        /// <param name="indexUri">The index URI.</param>
        /// <returns></returns>
        public IDBIndex GetIndex(Uri indexUri)
        {
            return _IndexLookup[indexUri.GetIndexName()];
        }


        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return Uri.GetCollectionName(); }
        }


        /// <summary>
        /// Gets the full name. (database.collection)
        /// </summary>
        /// <value>The full name.</value>
        public string FullName
        {
            get { return Uri.GetFullCollectionName(); }
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<IDocument> GetEnumerator()
        {
            return this.Find(DBQuery.SelectAll, null, null, null).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Drops the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        public void DropIndex(IDBIndex index)
        {
            if (ReadOnly)
                throw new ReadOnlyException("Cannot drop indexes when using a readonly binding");
            Database.deleteIndexes(this, index.Name);
            _IndexLookup.Remove(index.Name);
        }
    }
}
