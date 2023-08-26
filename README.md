# Crawler.Demo

## Dependency Injection
DI is great when you have a volatile dependency. I believe that for this crawler, it is not necessary.
As great of a tool DI is, it may reduce code readability due to the abstraction and injection of classes.
As such, I have decided to avoid using DI for this project.

## Why use a single HttpClient?
More info: https://www.aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/

## Not Supported
- Uris with Parameters
- File-type Uris