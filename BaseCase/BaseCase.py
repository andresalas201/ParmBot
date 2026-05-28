import NewsFetcher
import Summarizer



def main():
    query = input("Que quiere buscar? ")
    articles = NewsFetcher.fetch_main(query)
    if not articles:
        print("No articles found.")
        return
    summary = Summarizer.summarize(articles)
    print("=============================")
    print(summary)
    print("=============================")
    print("Articulos usados:")
    for i in articles:
        print(f"{i.get("title")}: {i.get("url")}")
        print("")


    


if __name__ == "__main__":
    main()