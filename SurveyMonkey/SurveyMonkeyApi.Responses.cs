﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using SurveyMonkey.Containers;
using SurveyMonkey.Helpers;
using SurveyMonkey.RequestSettings;

namespace SurveyMonkey
{
    public partial class SurveyMonkeyApi
    {
        public enum ObjectType
        {
            Survey,
            Collector
        }

        public Response GetResponseOverview(int objectId, ObjectType objectType, int responseId)
        {
            return GetResponseRequest(objectId, objectType, responseId, false);
        }

        public Response GetResponseDetail(int objectId, ObjectType objectType, int responseId)
        {
            return GetResponseRequest(objectId, objectType, responseId, true);
        }

        private Response GetResponseRequest(int objectId, ObjectType source, int responseId, bool details)
        {
            var detailString = details ? "/details" : String.Empty;
            string endPoint = String.Format("https://api.surveymonkey.net/v3/{0}s/{1}/responses/{2}", source.ToString().ToLower(), objectId, responseId, detailString);
            var verb = Verb.GET;
            JToken result = MakeApiRequest(endPoint, verb, new RequestData());
            var responses = result["data"].ToObject<Response>();
            return responses;
        }

        public List<Response> GetResponseOverviewList(int objectId, ObjectType objectType)
        {
            var settings = new GetResponseListSettings();
            return GetResponseListPager(objectId, objectType, settings, false);
        }

        public List<Response> GetResponseDetailList(int objectId, ObjectType objectType)
        {
            var settings = new GetResponseListSettings();
            return GetResponseListPager(objectId, objectType, settings, true);
        }

        public List<Response> GetResponseOverviewList(int objectId, ObjectType objectType, GetResponseListSettings settings)
        {
            return GetResponseListPager(objectId, objectType, settings, false);
        }

        public List<Response> GetResponseDetailList(int objectId, ObjectType objectType, GetResponseListSettings settings)
        {
            return GetResponseListPager(objectId, objectType, settings, true);
        }

        private List<Response> GetResponseListPager(int id, ObjectType objectType, GetResponseListSettings settings, bool details)
        {
            //Get the specific page & quantity
            if (settings.Page.HasValue || settings.PerPage.HasValue)
            {
                var requestData = PropertyCasingHelper.GetPopulatedProperties(settings);
                return GetResponseListRequest(id, objectType, details, requestData);
            }

            //Auto-page
            const int maxResultsPerPage = 1000;
            var results = new List<Response>();
            bool cont = true;
            int page = 1;
            while (cont)
            {
                settings.Page = page;
                settings.PerPage = maxResultsPerPage;
                var requestData = PropertyCasingHelper.GetPopulatedProperties(settings);
                var newResults = GetResponseListRequest(id, objectType, details, requestData);
                if (newResults.Count > 0)
                {
                    results.AddRange(newResults);
                }
                if (newResults.Count < maxResultsPerPage)
                {
                    cont = false;
                }
                page++;
            }
            return results;
        }

        private List<Response> GetResponseListRequest(int id, ObjectType objectType, bool details, RequestData requestData)
        {
            var bulk = details ? "/bulk" : String.Empty;
            string endPoint = String.Format("https://api.surveymonkey.net/v3/{0}s/{1}/responses{2}", objectType.ToString().ToLower(), id, bulk);
            var verb = Verb.GET;
            JToken result = MakeApiRequest(endPoint, verb, requestData);
            var responses = result["data"].ToObject<List<Response>>();
            return responses;
        }
    }
}