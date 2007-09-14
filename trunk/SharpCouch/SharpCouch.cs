// SharpCouch - a simple wrapper class for the CouchDB HTTP API
// Copyright 2007 Ciaran Gultnieks
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

using System;
using System.IO;  
using System.Net;  
using System.Text;
using System.Collections.Generic;
using LitJson;

namespace SharpCouch
{

	/// <summary>
	/// Used to return metadata about a document.
	/// </summary>
	public class DocInfo
	{
		public string ID;
		public string Revision;
	}
	
	/// <summary>
	/// A simple wrapper class for the CouchDB HTTP API. No
	/// initialisation is necessary, just create an instance and
	/// call the appropriate methods to interact with CouchDB.
	/// All methods throw exceptions when things go wrong.
	/// </summary>
	public class DB
	{
		public DB()
		{
		}

		/// <summary>
		/// Get a list of database on the server.
		/// </summary>
		/// <param name="server">The server URL</param>
		/// <returns>A string array containing the database names
		/// </returns>
		public string[] GetDatabases(string server)
		{
			string result=DoRequest(server+"/_all_dbs","GET");

			JsonData d=JsonMapper.ToObject(result);
			List<string> list=new List<string>();
			foreach(JsonData db in d)
				list.Add(db.ToString());
			return(list.ToArray());			
		}
	
		/// <summary>
		/// Get the document count for the given database.
		/// </summary>
		/// <param name="server">The server URL</param>
		/// <param name="db">The database name</param>
		/// <returns>The number of documents in the database</returns>
		public int CountDocuments(string server,string db)
		{
			// I don't know a more efficient way of doing this at
			// present other than getting a list of all documents...
			string result=DoRequest(server+"/"+db+"/_all_docs","GET");

			JsonData d=JsonMapper.ToObject(result);
			int count=d["rows"].Count;
			return count;
		}

		
		/// <summary>
		/// Get information on all the documents in the given database.
		/// </summary>
		/// <param name="server">The server URL</param>
		/// <param name="db">The database name</param>
		/// <returns>An array of DocInfo instances</returns>
		public DocInfo[] GetAllDocuments(string server,string db)
		{
			string result=DoRequest(server+"/"+db+"/_all_docs","GET");
			
			List<DocInfo> list=new List<DocInfo>();

			JsonData d=JsonMapper.ToObject(result);
			foreach(JsonData row in d["rows"])
			{
				DocInfo doc=new DocInfo();
				doc.ID=row["_id"].ToString();
				doc.Revision=row["_rev"].ToString();
				list.Add(doc);
			}			
			return list.ToArray();
		}
		
		/// <summary>
		/// Create a new database.
		/// </summary>
		/// <param name="server">The server URL</param>
		/// <param name="db">The database name</param>
		public void CreateDatabase(string server,string db)
		{
			string result=DoRequest(server+"/"+db,"PUT");
			if(result!="{\"ok\":true}")
				throw new ApplicationException("Failed to create database: "+result);
		}

		/// <summary>
		/// Delete a database
		/// </summary>
		/// <param name="server">The server URL</param>
		/// <param name="db">The name of the database to delete</param>
		public void DeleteDatabase(string server,string db)
		{
			string result=DoRequest(server+"/"+db,"DELETE");
			if(result!="{\"ok\":true}")
				throw new ApplicationException("Failed to delete database: "+result);
		}

		/// <summary>
		/// Execute a temporary view and return the results.
		/// </summary>
		/// <param name="server">The server URL</param>
		/// <param name="db">The database name</param>
		/// <param name="viewdef">The javascript view definition</param>
		/// <returns>The result (JSON format)</returns>
		public string ExecTempView(string server,string db,string viewdef)
		{
			return DoRequest(server+"/"+db+"/_temp_view","POST",viewdef,"application/javascript");
		}
		
		/// <summary>
		/// Create a new document. If the document has no ID field,
		/// it will be assigned one by the server.
		/// </summary>
		/// <param name="server">The server URL</param>
		/// <param name="db">The database name</param>
		/// <param name="content">The document contents (JSON).</param>
		public void CreateDocument(string server,string db,string content)
		{
			DoRequest(server+"/"+db,"POST",content,"application/json");
		}

		/// <summary>
		/// Get a document.
		/// </summary>
		/// <param name="server">The server URL</param>
		/// <param name="db">The database name</param>
		/// <param name="docid">The document ID.</param>
		/// <returns>The document contents (JSON)</returns>
		public string GetDocument(string server,string db,string docid)
		{
			return DoRequest(server+"/"+db+"/"+docid,"GET");
		}

		/// <summary>
		/// Delete a document.
		/// </summary>
		/// <param name="server">The server URL</param>
		/// <param name="db">The database name</param>
		/// <param name="docid">The document ID.</param>
		public void DeleteDocument(string server,string db,string docid)
		{
			DoRequest(server+"/"+db+"/"+docid,"DELETE");
		}
		
		/// <summary>
		/// Internal helper to make an HTTP request and return the
		/// response. Throws an exception in the event of any kind
		/// of failure. Overloaded - use the other version if you
		/// need to post data with the request.
		/// </summary>
		/// <param name="url">The URL</param>
		/// <param name="method">The method, e.g. "GET"</param>
		/// <returns>The server's response</returns>
		private string DoRequest(string url,string method)
		{
			return DoRequest(url,method,null,null);
		}
		
		/// <summary>
		/// Internal helper to make an HTTP request and return the
		/// response. Throws an exception in the event of any kind
		/// of failure. Overloaded - use the other version if no
		/// post data is required.
		/// </summary>
		/// <param name="url">The URL</param>
		/// <param name="method">The method, e.g. "GET"</param>
		/// <param name="postdata">Data to be posted with the request,
		/// or null if not required.</param>
		/// <param name="contenttype">The content type to send, or null
		/// if not required.</param>
		/// <returns>The server's response</returns>
		private string DoRequest(string url,string method,string postdata,string contenttype)
		{
			HttpWebRequest req=WebRequest.Create(url) as HttpWebRequest;
			req.Method=method;
			if(contenttype!=null)
				req.ContentType=contenttype;
			
			if(postdata!=null)
			{
				byte[] bytes=UTF8Encoding.UTF8.GetBytes(postdata.ToString());
				req.ContentLength=bytes.Length;  
				using(Stream ps = req.GetRequestStream())
				{
					ps.Write(bytes, 0, bytes.Length);
				}
			}

			HttpWebResponse resp=req.GetResponse() as HttpWebResponse;
			string result;
			using(StreamReader reader=new StreamReader(resp.GetResponseStream()))
			{
				result=reader.ReadToEnd();
			}
			return result;
		}		
	
	}
}
