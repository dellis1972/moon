/*
 * DeepZoomImageTileSource.cs.
 *
 * Contact:
 *   Moonlight List (moonlight-list@lists.ximian.com)
 *
 * Copyright 2008 Novell, Inc. (http://www.novell.com)
 *
 * See the LICENSE file included with the distribution for details.
 * 
 */

using Mono;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Xml;

namespace System.Windows.Media
{	
	public sealed partial class DeepZoomImageTileSource : MultiScaleTileSource
	{
		public DeepZoomImageTileSource (Uri sourceUri) : this ()
		{
			UriSource = sourceUri;
		}

		protected override void GetTileLayers (int tileLevel, int tilePositionX, int tilePositionY, IList<object> tileImageLayerSources)
		{
			//FIXME
			//P/Invoke the native GetTileLayer
		}
	}
}
