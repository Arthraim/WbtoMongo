using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using RestSharp;
using System.IO.IsolatedStorage;
using System.Collections.Generic;

namespace WbtoMongo
{
    public static class WbtoApi
    {
        private static RestClient setAuthentication(RestClient client)
        {
            if (Session.hasUsernameAndPassword())
            {
                client.Authenticator = new HttpBasicAuthenticator(Session.username, Session.password);
            }
            return client;
        }

        public static RestClient GetClient(String username, String password)
        {
            RestClient client = new RestClient();
            client.BaseUrl = "http://wbto.cn";
            client.Authenticator = new HttpBasicAuthenticator(username, password);
            return client;
        }
        public static RestClient GetClient()
        {
            RestClient client = new RestClient();
            client.BaseUrl = "http://wbto.cn";
            client = setAuthentication(client);
            return client;
        }

        public static void upload(String content, String filename, byte[] file, Action<RestResponse> callback)
        {
            RestClient client = WbtoApi.GetClient();

            RestRequest request = new RestRequest("/api/upload.json", Method.POST);
            request.AddParameter("source", Constants.SOURCE);
            request.AddParameter("content", content);
            request.AddFile("file", file, filename);

            client.ExecuteAsync(request, callback);
        }

        public static void update(String content, Action<RestResponse> callback)
        {
            RestClient client = WbtoApi.GetClient();

            RestRequest request = new RestRequest("/api/update.json", Method.POST);
            request.AddParameter("source", Constants.SOURCE);
            request.AddParameter("content", content);

            client.ExecuteAsync(request, callback);
        }

        public static void login(String username, String password, Action<RestResponse> callback)
        {
            RestClient client = WbtoApi.GetClient(username, password);

            RestRequest request = new RestRequest("/api/account/verify_credentials.json", Method.POST);
            request.AddParameter("source", Constants.SOURCE);

            client.ExecuteAsync(request, callback);
        }

    }
}
