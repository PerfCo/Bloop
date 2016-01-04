using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Twitter.Infrastructure.Contracts;
using Twitter.Infrastructure.Contracts.Models;

namespace Twitter.Infrastructure
{
    public sealed class TwitterClient : OAuthSupport
    {
        private const string StreamFilterUrl = @"https://stream.twitter.com/1.1/statuses/filter.json";
        private readonly string _accessSecret;
        private readonly string _accessToken;

        private readonly string _consumerKey;
        private readonly string _consumerSecret;
        private readonly List<string> _followUserIds;

        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            StringEscapeHandling = StringEscapeHandling.EscapeHtml,
            DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
        };

        private readonly List<string> _trackKeywords;

        private readonly ITwitterObserver _twitterObserver;
        private StreamReader _responseStream;

        private HttpWebRequest _webRequest;
        private HttpWebResponse _webResponse;

        public TwitterClient(TwetterConfig config, ITwitterObserver twitterObserver)
        {
            if (config == null)
            {
                throw new ArgumentNullException();
            }
            if (config.TrackKeywords.Count == 0 && config.FollowUserIds.Count == 0)
            {
                throw new ConfigurationErrorsException();
            }

            _consumerKey = config.ConsumerKey;
            _consumerSecret = config.ConsumerSecret;
            _accessToken = config.AccessToken;
            _accessSecret = config.AccessSecret;
            _trackKeywords = config.TrackKeywords;
            _followUserIds = config.FollowUserIds;
            _twitterObserver = twitterObserver;
        }

        public async void Start2()
        {
            string postparameters = GetParameters();
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite);

                var request = new HttpRequestMessage(HttpMethod.Post, StreamFilterUrl);
                request.Headers.Add("Authorization", GetAuthHeader(StreamFilterUrl + "?" + postparameters));

                byte[] bytes = Encoding.UTF8.GetBytes(postparameters);
                request.Content = new ByteArrayContent(bytes);
                request.Content.Headers.ContentLength = bytes.Length;
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                using (
                    HttpResponseMessage response =
                        await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                {
                    using (Stream body = await response.Content.ReadAsStreamAsync())
                    {
                        using (var reader = new StreamReader(body))
                        {
                            while (!reader.EndOfStream)
                            {
                                try
                                {
                                    string rawTweet = reader.ReadLine();
                                    var tweet = JsonConvert.DeserializeObject<Tweet>(rawTweet, _serializerSettings);
                                    _twitterObserver.Notify(tweet);
                                }
                                catch (JsonSerializationException ex)
                                {
                                    Console.WriteLine(ex.ToString());
                                }
                                catch (JsonReaderException ex)
                                {
                                    Console.WriteLine(ex.ToString());
                                }
                            }
                        }
                    }
                }
            }
        }

        public async Task Start()
        {
            var wait = 250;

            string postparameters = GetParameters();

            try
            {
                _webRequest = (HttpWebRequest) WebRequest.Create(StreamFilterUrl);
                _webRequest.Timeout = -1;
                _webRequest.Headers.Add("Authorization", GetAuthHeader(StreamFilterUrl + "?" + postparameters));

                AddParams(postparameters);

                _webRequest.BeginGetResponse(ar =>
                {
                    var request = (WebRequest) ar.AsyncState;

                    using (WebResponse response = request.EndGetResponse(ar))
                    {
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            while (!reader.EndOfStream)
                            {
                                try
                                {
                                    string json = reader.ReadLine();
                                    var tweet = JsonConvert.DeserializeObject<Tweet>(json, _serializerSettings);
                                    _twitterObserver.Notify(tweet);
                                }
                                catch (JsonSerializationException jsonSEx)
                                {
                                    Debug.WriteLine(jsonSEx.ToString());
                                }
                                catch (JsonReaderException jsonEx)
                                {
                                    Debug.WriteLine(jsonEx.ToString());
                                }
                            }
                        }
                    }
                }, _webRequest);
            }
            catch (WebException ex)
            {
                Console.WriteLine(ex);
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    //-- From Twitter Docs -- 
                    //When a HTTP error (> 200) is returned, back off exponentially. 
                    //Perhaps start with a 10 second wait, double on each subsequent failure, 
                    //and finally cap the wait at 240 seconds. 
                    //Exponential Backoff
                    if (wait < 10000)
                    {
                        wait = 10000;
                    }
                    else
                    {
                        if (wait < 240000)
                        {
                            wait = wait*2;
                        }
                    }
                }
                else
                {
                    //-- From Twitter Docs -- 
                    //When a network error (TCP/IP level) is encountered, back off linearly. 
                    //Perhaps start at 250 milliseconds and cap at 16 seconds.
                    //Linear Backoff
                    if (wait < 16000)
                    {
                        wait += 250;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            finally
            {
                _webRequest?.Abort();
                if (_responseStream != null)
                {
                    _responseStream.Close();
                    _responseStream = null;
                }

                if (_webResponse != null)
                {
                    _webResponse.Close();
                    _webResponse = null;
                }
                Thread.Sleep(wait);
            }
        }

        private string GetParameters()
        {
            string trackKeywords = UrlEncode(string.Join(",", _trackKeywords.ToArray()));
            string followUserId = UrlEncode(string.Join(",", _followUserIds.ToArray()));

            string postparameters = (trackKeywords.Length == 0 ? string.Empty : "&track=" + trackKeywords) +
                                    (followUserId.Length == 0 ? string.Empty : "&follow=" + followUserId);

            if (postparameters.IndexOf('&') == 0)
            {
                postparameters = postparameters.Remove(0, 1).Replace("#", "%23");
            }
            return postparameters;
        }

        private void AddParams(string postparameters)
        {
            _webRequest.Method = "POST";
            _webRequest.ContentType = "application/x-www-form-urlencoded";

            byte[] bytes = Encoding.UTF8.GetBytes(postparameters);

            _webRequest.ContentLength = bytes.Length;
            using (Stream stream = _webRequest.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Close();
            }
        }

        private string GetAuthHeader(string url)
        {
            string normalizedString;
            string normalizeUrl;
            string timeStamp = GenerateTimeStamp();
            string nonce = GenerateNonce();


            string oauthSignature = GenerateSignature(new Uri(url), _consumerKey, _consumerSecret, _accessToken,
                _accessSecret, "POST", timeStamp, nonce, out normalizeUrl, out normalizedString);


            // create the request header
            const string headerFormat = "OAuth oauth_nonce=\"{0}\", oauth_signature_method=\"{1}\", " +
                                        "oauth_timestamp=\"{2}\", oauth_consumer_key=\"{3}\", " +
                                        "oauth_token=\"{4}\", oauth_signature=\"{5}\", " +
                                        "oauth_version=\"{6}\"";

            return string.Format(headerFormat,
                Uri.EscapeDataString(nonce),
                Uri.EscapeDataString(Hmacsha1SignatureType),
                Uri.EscapeDataString(timeStamp),
                Uri.EscapeDataString(_consumerKey),
                Uri.EscapeDataString(_accessToken),
                Uri.EscapeDataString(oauthSignature),
                Uri.EscapeDataString(OAuthVersion));
        }
    }
}