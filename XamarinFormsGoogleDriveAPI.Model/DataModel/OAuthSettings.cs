﻿using System;

namespace XamarinFormsGoogleDriveAPI.Model.DataModel
{
	public class OAuthSettingsModel
	{
		public OAuthSettingsModel(
			string clientId,
			string scope,
			string authorizeUrl,
			string redirectUrl)
		{
			ClientId = clientId;
			Scope = scope;
			AuthorizeUrl = authorizeUrl;
			RedirectUrl = redirectUrl;
		}

		public string ClientId {get; private set;}
		public string Scope {get; private set;}
		public string AuthorizeUrl {get; private set;}
		public string RedirectUrl {get; private set;}
	}
}

