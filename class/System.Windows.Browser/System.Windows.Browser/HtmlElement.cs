//
// HtmlElement.cs
//
// Contact:
//   Moonlight List (moonlight-list@lists.ximian.com)
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Security;

namespace System.Windows.Browser
{
	public class HtmlElement : HtmlObject
	{
		// When does this .ctor make sense?
		internal HtmlElement ()
		{
		}

		internal HtmlElement (IntPtr handle)
			: base (handle)
		{
		}

		[SecuritySafeCritical ()]
		public void AppendChild (HtmlElement element)
		{
			InvokeInternal<object> (Handle, "appendChild", element);
		}

		[SecuritySafeCritical ()]
		public void AppendChild (HtmlElement element, HtmlElement referenceElement)
		{
			InvokeInternal<object> (Handle, "insertBefore", element, referenceElement);
		}

		[SecuritySafeCritical ()]
		public void Focus ()
		{
			InvokeInternal<object> (Handle, "focus");
		}

		public string GetAttribute (string name)
		{
			return InvokeInternal<string> (Handle, "getAttribute", name);
		}

		[MonoTODO]
		public string GetStyleAttribute (string name)
		{
			throw new NotImplementedException ();
		}

		public void RemoveAttribute (string name)
		{
			InvokeInternal<object> (Handle, "removeAttribute", name);
		}

		[SecuritySafeCritical ()]
		public void RemoveChild (HtmlElement element)
		{
			InvokeInternal<object> (Handle, "removeChild", element);
		}

				
		[MonoTODO]
		public void RemoveStyleAttribute (string name)
		{
			throw new NotImplementedException ();
		}

		public void SetAttribute (string name, string value)
		{
			InvokeInternal<object> (Handle, "setAttribute", name, value);
		}

		[MonoTODO ("This doesn't seem to work.")]
		public void SetStyleAttribute (string name, string value)
		{
			IntPtr style = GetPropertyInternal<IntPtr> (Handle, "style");
			SetPropertyInternal (style, name, value);
		}

		public ScriptObjectCollection Children {
			[SecuritySafeCritical]
			get { return new ScriptObjectCollection ((IntPtr) GetProperty ("childNodes")); }
		}

		public string CssClass {
			get { return GetPropertyInternal<string> (Handle, "class"); }
			set { SetPropertyInternal (Handle, "class", value); }
		}

		public string Id {
			get { return GetPropertyInternal<string> (Handle, "id"); }
			set { SetPropertyInternal (Handle, "id", value); }
		}

		public HtmlElement Parent {
			[SecuritySafeCritical]
			get { return new HtmlElement (GetPropertyInternal<IntPtr> (Handle, "parentNode")); }
		}

		public string TagName {
			get { return GetPropertyInternal<string> (Handle, "tagName"); }
		}
	}
}

