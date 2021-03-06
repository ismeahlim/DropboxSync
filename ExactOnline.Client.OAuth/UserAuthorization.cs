﻿using DotNetOpenAuth.OAuth2;
using System;

namespace ExactOnline.Client.OAuth
{
	public class UserAuthorization
	{
		public string AccessToken
		{
			get { return AuthorizationState.AccessToken; }
		}

		public string RefreshToken { get; set; }

		public IAuthorizationState AuthorizationState { get; set; }
	}
}