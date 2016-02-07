# OAuth2 Social Authentication

[![Build status](https://ci.appveyor.com/api/projects/status/hqk9wkg54oyphapn?svg=true)](https://ci.appveyor.com/project/akornatskyy/oauth2-social-net)

A simple web API written using [ASP.NET Web API](http://www.asp.net/web-api)
and [Microsoft OWIN](http://katanaproject.codeplex.com/)
for OAuth2 social authentication with Google or Facebook.

## Prepare

1. In web.config change app settings for:
	- *Google.ClientId* and *Google.ClientSecret*.
	- *Facebook.AppId* and *Facebook.AppSecret*.
1. OAuth2 redirect callbacks:
	- http://localhost:54325/oauth2/google/callback
	- http://localhost:54325/oauth2/facebook/callback
1. Support CORS, update *urls* in *index.html*. See
more about using custom domains with IIS Express [here](http://stackoverflow.com/questions/4709014/using-custom-domains-with-iis-express).