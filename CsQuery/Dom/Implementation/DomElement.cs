﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using CsQuery.Utility;
using CsQuery.ExtensionMethods;
using CsQuery.ExtensionMethods.Internal;

namespace CsQuery.Implementation
{

    /// <summary>
    /// HTML elements
    /// </summary>
    public class DomElement : DomContainer<DomElement>, IDomElement
    {
        #region private fields
        private DomAttributes _DomAttributes;
        protected CSSStyleDeclaration _Style;
        protected List<ushort> _Classes;

        protected DomAttributes DomAttributes
        {
            get
            {
                if (_DomAttributes == null)
                {
                    _DomAttributes = new DomAttributes();
                }
                return _DomAttributes;
            }
        }
        #endregion

        #region constructors
        public DomElement()
        {

        }
        public DomElement(string tag)
            : base()
        {
            NodeName = tag;
        }
        public DomElement(ushort tagId)
            : base()
        {
            _NodeNameID = tagId;
        }


        #endregion
        
        #region public properties
        public override CSSStyleDeclaration Style
        {
            get
            {
                if (_Style == null)
                {
                    _Style = new CSSStyleDeclaration(this);
                }
                return _Style;
            }
            protected set
            {
                _Style = value;
            }
        }
        public override IEnumerable<KeyValuePair<string,string>> Attributes
        {
            get
            {
                if (_DomAttributes == null)
                {
                    _DomAttributes = new DomAttributes();
                }
                return _DomAttributes;
            }
        }

        public override string ClassName
        {
            get
            {
                if (HasClasses)
                {
                    //return String.Join(" ", _Classes.Select(item=>DomData.TokenName(item)));
                    string className = "";
                    foreach (ushort clsId in _Classes)
                    {
                        className += (className == "" ? "" : " ") + DomData.TokenName(clsId);
                    }
                    return className;
                }
                else
                {
                    return "";
                }
            }
            set
            {
                SetClassName(value);
            }
        }
        public override string Id
        {
            get
            {
                return GetAttribute(DomData.IDAttrId, String.Empty);
            }
            set
            {
                if (!IsDisconnected)
                {
                    if (DomAttributes.ContainsKey(DomData.IDAttrId))
                    {
                        Document.RemoveFromIndex(IndexKey("#", DomData.TokenID(Id), Path));
                    }
                    if (value != null)
                    {
                        Document.AddToIndex(IndexKey("#", DomData.TokenID(value), Path), this);
                    }
                }
                SetAttributeRaw(DomData.IDAttrId, value);
            }
        }

        /// <summary>
        /// The NodeName for the element (upper case).
        /// </summary>
        public override string NodeName
        {
            get
            {
                return DomData.TokenName(_NodeNameID).ToUpper();
            }
            set
            {
                if (_NodeNameID < 1)
                {
                    _NodeNameID = DomData.TokenID(value, true);
                }
                else
                {
                    throw new InvalidOperationException("You can't change the tag of an element once it has been created.");
                }
            }
        }
        /// <summary>
        /// TODO: in HTML5 type can be used on OL attributes (and maybe others?) and its value is
        /// case sensitive. The Type of input elements is always lower case, though. This behavior
        /// needs to be verified against the spec
        /// </summary>
        public override string Type
        {
            get
            {
                return NodeName=="INPUT" ?
                    GetAttribute("type").ToLower() :
                    GetAttribute("type");
            }
            set
            {
                SetAttribute("type", value);
            }
        }

        /// <summary>
        /// For certain elements, the Name. TODO: Verify attribute is applicable.
        /// </summary>
        public override string Name
        {
            get
            {
                return GetAttribute("name");
            }
            set
            {
                SetAttribute("name", value);
            }
        }
        public override string DefaultValue
        {
            get
            {
                return hasDefaultValue() ?
                    (NodeName == "TEXTAREA" ? 
                        InnerText : 
                        GetAttribute("value")) :
                    base.DefaultValue;
            }
            set
            {
                if (!hasDefaultValue())
                {
                    base.DefaultValue = value;
                }
                else
                {
                    if (NodeName == "TEXTAREA")
                    {
                        InnerText = value;
                    }
                    else
                    {
                        SetAttribute("value",value);
                    }
                }
            }
        }

        
        /// <summary>
        /// Value property for some node types (input,textarea)
        /// </summary>
        public override string Value
        {
            get
            {
                return DomData.tagINPUT == _NodeNameID &&
                    HasAttribute(DomData.ValueAttrId) ?
                        GetAttribute(DomData.ValueAttrId) :
                        null;
            }
            set
            {
                SetAttribute(DomData.ValueAttrId, value);
            }
        }
        public override NodeType NodeType
        {
            get { return NodeType.ELEMENT_NODE; }
        }
        public override IDomContainer ParentNode
        {
            get
            {
                return base.ParentNode;
            }
            internal set
            {
                base.ParentNode = value;
            }
        }

        public override bool HasAttributes
        {
            get
            {
                return _DomAttributes != null && DomAttributes.HasAttributes;
            }
        }
        public override bool HasStyles
        {
            get
            {
                return _Style != null && _Style.Count > 0;
            }
        }
        public override bool HasClasses
        {
            get
            {
                return _Classes != null && _Classes.Count > 0;
            }
        }
        
        public override string PathID
        {
            get
            {
                if (_PathID == null)
                {
                    _PathID = PathEncode(Index);
                }
                return _PathID;
            }
        }
        public override bool IsIndexed
        {
            get
            {
                return true;
            }
        }
        /// <summary>
        /// This object type can have inner HTML.
        /// </summary>
        /// <returns></returns>
        public override bool InnerHtmlAllowed
        {
            get
            {
                return !DomData.NoInnerHtmlAllowed(_NodeNameID);

            }
        }
        public override bool InnerTextAllowed
        {
            get
            {
                return DomData.InnerTextAllowed(_NodeNameID);
            }
        }
        /// <summary>
        /// True if this element is valid (it needs a tag only)
        /// </summary>
        public override bool Complete
        {
            get { return _NodeNameID >= 0; }
        }
        /// <summary>
        /// Returns the value of the named attribute
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override string this[string attribute]
        {
            get
            {

                return GetAttribute(attribute);
            }
            set
            {
                SetAttribute(attribute, value);
            }
        }
        public override IDomObject this[int index]
        {
            get
            {
                return ChildNodes[index];
            }
        }
        public override bool Selected
        {
            get
            {
                return HasAttribute(DomData.SelectedAttrId);
            }
        }
        public override bool Checked
        {
            get
            {
                return HasAttribute(DomData.CheckedAttrId);
            }
            set
            {
                SetAttribute(DomData.CheckedAttrId, value ? "" : null);
            }
        }
        public override bool ReadOnly
        {
            get
            {
                return HasAttribute(DomData.ReadonlyAttrId);
            }
            set
            {
                SetAttribute(DomData.ReadonlyAttrId, value ? "" : null);
            }
        }
        /// <summary>
        /// Returns text of the inner HTML. When setting, any children will be removed.
        /// </summary>
        public override string InnerHTML
        {
            get
            {
                if (!HasChildren)
                {
                    return String.Empty;
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    base.Render(sb, Document == null ? CQ.DefaultDomRenderingOptions : Document.DomRenderingOptions);
                    return sb.ToString();
                }
            }
            set
            {
                if (!InnerHtmlAllowed)
                {
                    throw new InvalidOperationException(String.Format("You can't set the innerHTML for a {0} element.", NodeName));
                }
                ChildNodes.Clear();

                CQ csq = CQ.Create(value);
                ChildNodes.AddRange(csq.Document.ChildNodes);
            }
        }
        public override string InnerText
        {
            get
            {
                if (!HasChildren)
                {
                    return String.Empty;
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (IDomObject elm in ChildNodes)
                    {
                        if (elm.NodeType == NodeType.TEXT_NODE)
                        {
                            elm.Render(sb);
                        }
                    }
                    return sb.ToString();
                }
            }
            set
            {
                if (!InnerTextAllowed)
                {
                    throw new InvalidOperationException(String.Format("You can't set the innerHTML for a {0} element.", NodeName));
                }
                IDomText text;
                if (!InnerHtmlAllowed)
                {
                    text = new DomInnerText(value);
                }
                else
                { 
                    text = new DomText(value);
                }
                ChildNodes.Clear();
                ChildNodes.Add(text);
            }
        }
        
        /// <summary>
        /// The index excluding text nodes
        /// </summary>
        public int ElementIndex
        {
            get
            {
                int index = -1;
                IDomElement el = this;
                while (el != null)
                {
                    el = el.PreviousElementSibling;
                    index++;
                }
                return index;
            }
        }

        /// <summary>
        /// The object to which this index refers
        /// </summary>
        public IDomObject IndexReference
        {
            get
            {
                return this;
            }
        }

        /// <summary>
        /// Returns true if this element is a block-trpe element
        /// </summary>
        public bool IsBlock
        {
            get
            {
                return DomData.IsBlock(_NodeNameID);
            }
        }

        /// <summary>
        /// All class names present for this element
        /// </summary>
        public IEnumerable<string> Classes
        {
            get
            {
                foreach (var id in _Classes)
                {
                    yield return DomData.TokenName(id);
                }
            }
        }
        #endregion
        
        #region public methods
        public void Reindex()
        {
            PathID = null;
            Index = 0;
        }

        /// <summary>
        /// Returns the completel HTML for this element and its children
        /// </summary>
        public override void Render(StringBuilder sb, DomRenderingOptions options)
        {
            GetHtml(options, sb, true);
        }
        /// <summary>
        /// Returns the HTML for this element, but ignoring children/innerHTML
        /// </summary>
        public string ElementHtml()
        {
            StringBuilder sb = new StringBuilder();
            GetHtml(Document == null ? CQ.DefaultDomRenderingOptions : Document.DomRenderingOptions, sb, false);
            return sb.ToString();
        }

        /// <summary>
        /// Returns all the keys that should be in the index for this item (keys for class, tag, attributes, and id)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> IndexKeys()
        {

            string path = Path;
            yield return ""+DomData.indexSeparator+path;
            yield return IndexKey("+",_NodeNameID, path);
            string id = Id;
            if (!String.IsNullOrEmpty(id))
            {
                yield return IndexKey("#" ,DomData.TokenID(id), path);
            }
            if (HasClasses)
            {
                foreach (ushort clsId in _Classes)
                {
                    yield return IndexKey(".", clsId, path);
                }
            }
            if (HasAttributes)
            {
                foreach (ushort attrId in DomAttributes.GetAttributeIds())
                {
                    yield return IndexKey("!", attrId, path);
                }
            }
        }
       
        public override DomElement Clone()
        {
            var clone = new DomElement();
            clone._NodeNameID = _NodeNameID;

            if (HasAttributes)
            {
                clone._DomAttributes = DomAttributes.Clone();
            }
            if (HasClasses)
            {
                clone._Classes = new List<ushort>(_Classes);
            }
            if (HasStyles)
            {
                clone.Style = Style.Clone(clone);
            }
            // will not create ChildNodes lazy object unless results are returned (this is why we don't use AddRange)
            foreach (IDomObject child in CloneChildren())
            {
                clone.ChildNodes.AddAlways(child);
            }

            return clone;
        }
        public override IEnumerable<IDomObject> CloneChildren()
        {
            if (_ChildNodes!=null)
            {
                foreach (IDomObject obj in ChildNodes)
                {
                    yield return obj.Clone();
                }
            }
            yield break;
        }
        public bool HasStyle(string name)
        {
            return HasStyles &&
                Style.HasStyle(name);
        }
        public bool HasClass(string name)
        {
            return HasClasses 
                && _Classes.Contains(DomData.TokenID(name));
        }

        public bool AddClass(string name)
        {
            bool result=false;
            bool hadClasses = HasClasses;

            foreach (string cls in name.SplitClean())
            {
                
                if (!HasClass(cls))
                {
                    if (_Classes == null)
                    {
                        _Classes = new List<ushort>();
                    }
                    ushort tokenId = DomData.TokenID(cls);
                    
                    _Classes.Add(tokenId);
                    if (!IsDisconnected)
                    {
                        Document.AddToIndex(IndexKey(".",tokenId), this);
                    }
                    
                    result = true;
                }
            }
            if (result && !hadClasses && !IsDisconnected)
            {
                // Must index the attributes for search just on attribute too
                Document.AddToIndex(AttributeIndexKey(DomData.ClassAttrId), this);
            }
            return result;
        }
        public bool RemoveClass(string name)
        {
            bool result = false;
            bool hasClasses = HasClasses;
            foreach (string cls in name.SplitClean())
            {
                if (HasClass(cls))
                {
                    ushort tokenId = DomData.TokenID(cls);
                    _Classes.Remove(tokenId);
                    if (!IsDisconnected)
                    {
                        Document.RemoveFromIndex(IndexKey(".",tokenId));
                    }

                    result = true;
                }
            }
            if (!HasClasses && hasClasses && !IsDisconnected)
            {
                Document.RemoveFromIndex(AttributeIndexKey(DomData.ClassAttrId));
            }

            return result;
        }

        /// <summary>
        /// Add a single style in the form "styleName: value"
        /// </summary>
        /// <param name="style"></param>
        public void AddStyle(string style)
        {
            AddStyle(style,true);
        }
        public void AddStyle(string style,bool strict)
        {
            Style.AddStyles(style, strict);
        }
        public bool RemoveStyle(string name)
        {
            return _Style != null ? Style.Remove(name) : false;
        }
        protected bool HasAttribute(ushort tokenId)
        {
            switch (tokenId)
            {
                case DomData.ClassAttrId:
                    return HasClasses;
                case DomData.tagSTYLE:
                    return HasStyles;
                default:
                    return _DomAttributes != null
                        && DomAttributes.ContainsKey(tokenId);
            }
        }
        public override bool HasAttribute(string name)
        {
            return HasAttribute(DomData.TokenID(name, true));
        }
        public void SetStyles(string styles)
        {
            SetStyles(styles, true);
        }
        public void SetStyles(string styles, bool strict)
        {
            Style.SetStyles(styles, strict);
        }
        public override void SetAttribute(string name, string value)
        {
            SetAttribute(DomData.TokenID(name,true),value);
        }

        protected void SetAttribute(ushort tokenId, string value)
        {
            switch (tokenId)
            {
                case DomData.ClassAttrId:
                    ClassName = value;
                    return;
                case DomData.IDAttrId:
                    Id = value;
                    break;
                case DomData.tagSTYLE:
                    Style.SetStyles(value, false);
                    return;
                default:
                    // Uncheck any other radio buttons
                    if (tokenId == DomData.CheckedAttrId
                        && _NodeNameID == DomData.tagINPUT
                        && Type == "radio"
                        && !String.IsNullOrEmpty(Name)
                        && value != null
                        && Document != null)
                    {
                        var radios = Document.QuerySelectorAll("input[type='radio'][name='" + Name + "']:checked");
                        foreach (var item in radios)
                        {
                            item.Checked = false;
                        }
                    }
                    break;
            }

            SetAttributeRaw(tokenId, value);

        }
        /// <summary>
        /// Sets an attribute with no value
        /// </summary>
        /// <param name="name"></param>
        public override void SetAttribute(string name)
        {
            SetAttribute(DomData.TokenID(name, true));
            
        }
        public void SetAttribute(ushort tokenId)
        {
            AttributeAddToIndex(tokenId);
            DomAttributes.SetBoolean(tokenId);
        }

        /// <summary>
        /// Used by DomElement to (finally) set the ID value
        /// </summary>
        /// <param name="tokenId"></param>
        /// <param name="value"></param>
        protected void SetAttributeRaw(ushort tokenId, string value)
        {
            if (value == null)
            {
                DomAttributes.Unset(tokenId);
                AttributeRemoveFromIndex(tokenId);
            }
            else
            {
                AttributeAddToIndex(tokenId);
                DomAttributes[tokenId] = value;
            }
        }
        public override bool RemoveAttribute(string name)
        {
            return RemoveAttribute(DomData.TokenID(name,true));

        }
        protected bool RemoveAttribute(ushort tokenId)
        {
            switch (tokenId)
            {
                case DomData.ClassAttrId:
                    if (HasClasses)
                    {
                        SetClassName(null);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case DomData.IDAttrId:
                    if (DomAttributes.ContainsKey(DomData.IDAttrId))
                    {
                        Id = null;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case DomData.tagSTYLE:
                    if (HasStyles)
                    {
                        foreach (var style in Style.Keys)
                        {
                            Style.Remove(style);
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                    }
            }

            if (_DomAttributes == null)
            {
                return false;
            }

            bool success = DomAttributes.Remove(tokenId);
            if (success)
            {
                AttributeRemoveFromIndex(tokenId);
            }
            return success;
            
        }
        /// <summary>
        /// Gets an attribute value, or returns null if the value is missing. If a valueless attribute is found, this will also return null. HasAttribute should be used
        /// to test for such attributes. Attributes with an empty string value will return String.Empty.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override string GetAttribute(string name)
        {
            return GetAttribute(name, null);
        }
        protected string GetAttribute(ushort tokenId)
        {
            return GetAttribute(tokenId, null);
        }
        /// <summary>
        /// Returns the value of an attribute or a default value if it could not be found.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override string GetAttribute(string name, string defaultValue)
        {
            return GetAttribute(DomData.TokenID(name,true),defaultValue);
        }
        protected string GetAttribute(ushort tokenId, string defaultValue)
        {

            string value = null;
            if (TryGetAttribute(tokenId, out value))
            {
                //IMPORTANT: Even though we need to distinguish between null and empty string values internally to
                // render the same way it was brought over (e.g. either "checked" or "checked=''") --- accessing the
                // attribute value is never null for attributes that exist.
                return value ?? "";
            }
            else
            {
                return defaultValue;
            }
        }
        public bool TryGetAttribute(ushort tokenId, out string value)
        {
            switch (tokenId)
            {
                case DomData.ClassAttrId:
                    value = ClassName;
                    return true;
                case DomData.tagSTYLE:
                    value = Style.ToString();
                    return true;
                default:
                    if (_DomAttributes != null) {
                        return DomAttributes.TryGetValue(tokenId, out value);
                    }
                    value = null;
                    return false;
            }
        }
        public override bool TryGetAttribute(string name, out string value)
        {
            return TryGetAttribute(DomData.TokenID(name,true), out value);
        }

        public override string ToString()
        {
            return ElementHtml();
        }

        #endregion

        #region private methods

        public string AttributeIndexKey(string attrName)
        {
            return AttributeIndexKey(DomData.TokenID(attrName, true));
        }
        public string AttributeIndexKey(ushort attrId)
        {
#if DEBUG_PATH
            return "!" + DomData.TokenName(attrId) + DomData.indexSeparator + Owner.Path;
#else
            return "!" + (char)attrId + DomData.indexSeparator + Path;
#endif
        }
        protected void AttributeRemoveFromIndex(ushort attrId)
        {
            if (!IsDisconnected)
            {
                Document.RemoveFromIndex(AttributeIndexKey(attrId));
            }
        }
        protected void AttributeAddToIndex(ushort attrId)
        {
            if (!IsDisconnected && !DomAttributes.ContainsKey(attrId))
            {
                
                Document.AddToIndex(AttributeIndexKey(attrId), this);
            }
        }

        protected void SetClassName(string className)
        {
            
            if (HasClasses) {
                foreach (var cls in Classes.ToList())
                {
                    RemoveClass(cls);
                }
            }
            if (!string.IsNullOrEmpty(className)) 
            {
                AddClass(className);
            }
            
        }
        protected bool hasDefaultValue()
        {
            return NodeNameID == DomData.tagINPUT || NodeNameID == DomData.tagTEXTAREA;
        }
        internal string IndexKey(string prefix, ushort keyTokenId)
        {
            return IndexKey(prefix, keyTokenId, Path);
        }
        internal string IndexKey(string prefix, string key)
        {
            return IndexKey(prefix, key, Path);
        }
        internal string IndexKey(string prefix, string key, string path)
        {
#if DEBUG_PATH
            return prefix + key + DomData.indexSeparator + path;
#else
            return IndexKey(prefix, DomData.TokenID(key,true), path);
#endif
        }
        internal string IndexKey(string prefix, ushort keyTokenId, string path)
        {
#if DEBUG_PATH
            return prefix + DomData.TokenName(keyTokenId) + DomData.indexSeparator + path;
#else
            return prefix + (char)keyTokenId + DomData.indexSeparator + path;
#endif
        }
    
        protected void GetHtml(DomRenderingOptions options, StringBuilder sb, bool includeChildren)
        {
            bool quoteAll = options.HasFlag(DomRenderingOptions.QuoteAllAttributes);

            sb.Append("<");
            string nodeName = NodeName.ToLower();
            sb.Append(nodeName);
            // put ID first. Must use GetAttribute since the Id property defaults to ""
            string id = GetAttribute(DomData.IDAttrId,null);
            
            if (id != null)
            {
                sb.Append(" ");
                RenderAttribute(sb, "id", id, quoteAll);
            }
            if (_Style != null && Style.Count > 0)
            {
                sb.Append(" style=\"");
                sb.Append(Style.ToString());
                sb.Append("\"");
            }
            if (HasClasses)
            {
                sb.Append(" class=\"");
                sb.Append(ClassName);
                sb.Append("\"");
            }

            if (_DomAttributes != null)
            {
                foreach (var kvp in _DomAttributes)
                {
                    if (kvp.Key != "id")
                    {
                        sb.Append(" ");
                        RenderAttribute(sb, kvp.Key, kvp.Value, quoteAll);
                    }
                }
            }
            if (InnerHtmlAllowed || InnerTextAllowed )
            {
                sb.Append(">");
                if (includeChildren)
                {
                    base.Render(sb, options);
                }
                else
                {
                    sb.Append(HasChildren ?
                            "..." :
                            String.Empty);
                }
                sb.Append("</");
                sb.Append(nodeName);
                sb.Append(">");
            }
            else
            {

                if ((Document == null ? CQ.DefaultDocType : Document.DocType)== DocType.XHTML)
                {
                    sb.Append(" />");
                }
                else
                {
                    sb.Append(">");
                }
            }
        }
        /// <summary>
        /// TODO this really should be in Attributes
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected void RenderAttribute(StringBuilder sb, string name, string value, bool quoteAll)
        {
            if (value != null)
            {
                string quoteChar;
                string attrText = DomData.AttributeEncode(value,
                    quoteAll,
                    out quoteChar);
                sb.Append(name.ToLower());
                sb.Append("=");
                sb.Append(quoteChar);
                sb.Append(attrText);
                sb.Append(quoteChar);
            }
            else
            {
                sb.Append(name);
            }
        }
        #endregion
    }
}
