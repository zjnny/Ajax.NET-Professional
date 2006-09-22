/*
 * MS	06-04-25	removed unnecessarily used cast
 * MS	06-05-23	using local variables instead of "new Type()" for get De-/SerializableTypes
 * MS	06-06-09	removed addNamespace use
 * MS	06-06-13	fixed if key includes blanks
 *					fixed __type is not a key
 *					fixed missing initial values (for AjaxMethod invoke return value!!)
 * MS	06-06-14	changed access to keys and values
 * MS	06-09-22	added inheritance to get HttpValueCollection working again
 * 
 * 
 */
using System;
using System.Text;
using System.Collections;
using System.Collections.Specialized;

namespace AjaxPro
{
	/// <summary>
	/// Provides methods to serialize and deserialize a NameValueCollection.
	/// </summary>
	public class NameValueCollectionConverter : IJavaScriptConverter
	{
		private string clientType = "Ajax.Web.NameValueCollection";

		public NameValueCollectionConverter()
			: base()
		{
			m_serializableTypes = new Type[] { typeof(NameValueCollection) };
			m_deserializableTypes = new Type[] { typeof(NameValueCollection) };

			m_AllowInheritance = true;
		}

		public override string GetClientScript()
		{
			return JavaScriptUtil.GetClientNamespaceRepresentation(clientType) + @"
" + clientType + @" = function(items) {
	this.__type = ""System.Collections.Specialized.NameValueCollection"";
	this.keys = [];
	this.values = [];

	if(items != null && !isNaN(items.length)) {
		for(var i=0; i<items.length; i++)
			this.add(items[i][0], items[i][1]);
	}
}
Object.extend(" + clientType + @".prototype, {
	add: function(k, v) {
		if(k == null || k.constructor != String || v == null || v.constructor != String)
			return -1;
		this.keys.push(k);
		this.values.push(v);
		return this.values.length -1;
	},
	containsKey: function(key) {
		for(var i=0; i<this.keys.length; i++)
			if(this.keys[i] == key) return true;
		return false;
	},
	getKeys: function() {
		return this.keys;
	},
	getValue: function(k) {
		for(var i=0; i<this.keys.length && i<this.values.length; i++)
			if(this.keys[i] == k) return this.values[i];
		return null;
	},
	setValue: function(k, v) {
		if(k == null || k.constructor != String || v == null || v.constructor != String)
			return -1;
		for(var i=0; i<this.keys.length && i<this.values.length; i++) {
			if(this.keys[i] == k) this.values[i] = v;
			return i;
		}
		return this.add(k, v);
	},
	toJSON: function() {
		return AjaxPro.toJSON({__type:this.__type,keys:this.keys,values:this.values});
	}
}, true);
";
		}

		public override object Deserialize(IJavaScriptObject o, Type t)
		{
			JavaScriptObject jso = o as JavaScriptObject;
			if (!typeof(NameValueCollection).IsAssignableFrom(t) || jso == null)
				throw new NotSupportedException();

			NameValueCollection list = (NameValueCollection)Activator.CreateInstance(t);

			if (!jso.Contains("keys") || !jso.Contains("values"))
				throw new ArgumentException("Missing values for 'keys' and 'values'.");

			JavaScriptArray keys = (JavaScriptArray)jso["keys"];
			JavaScriptArray values = (JavaScriptArray)jso["values"];

			if (keys.Count != values.Count)
				throw new IndexOutOfRangeException("'keys' and 'values' have different length.");

			for(int i=0; i<keys.Count; i++)
			{
				list.Add(keys[i].ToString(), values[i].ToString());
			}

			return list;
		}

		public override string Serialize(object o)
		{
			NameValueCollection list = o as NameValueCollection;

			if (list == null)
				throw new NotSupportedException();

			StringBuilder sb = new StringBuilder();

			bool b = true;

			sb.Append("new ");
			sb.Append(clientType);
			sb.Append("([");

			for (int i = 0; i < list.Keys.Count; i++)
			{
				if (b) { b = false; }
				else { sb.Append(","); }

				sb.Append('[');
				sb.Append(JavaScriptSerializer.Serialize(list.Keys[i]));
				sb.Append(',');
				sb.Append(JavaScriptSerializer.Serialize(list[list.Keys[i]]));
				sb.Append(']');
			}

			sb.Append("])");

			return sb.ToString();
		}
	}
}
