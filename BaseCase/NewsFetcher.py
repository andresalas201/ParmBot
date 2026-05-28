import os
import requests
import json
from datetime import datetime, timedelta, timezone

configFile = open("ParmBot/Assets/Resources/config.json")
jsonFile = json.load(configFile)
news_key = jsonFile["news_key"]
NEWSAPI_ENDPOINT = "https://eventregistry.org/api/v1/article/getArticles"
MAX_EVENTS = 5

def fetch_events(query):
    payload = {
        "action": "getArticles",
        "query": {
            "$query": {
                "keyword": query,
                "keywordLoc": "body",
                "lang": "spa"
            }
        },
        "resultType": "articles",
        "articlesSortBy": "date",
        "articlesSortByAsc": False,
        "articlesCount": MAX_EVENTS,
        "includeArticleTitle": True,
        "includeArticleKeywords": True,
        "includeArticleUrl": True,
        "includeArticleBody": True,
        "articleBodyLen": 3000,
        "apiKey": news_key,
    }
    response = requests.post(
        NEWSAPI_ENDPOINT,
        json=payload,
        headers={"Content-Type": "application/json"},
        timeout=30,
    )
    response.raise_for_status()
    data = response.json()
    articles = data.get("articles", {}).get("results", [])
    return articles

def fetch_main(query):
    articles = fetch_events(query)
    return articles