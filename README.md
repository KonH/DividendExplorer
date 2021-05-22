# Dividend Explorer

Share dividend collector using Yahoo Finance API from Saint-Petersburg input format with 30 years limit.

Usage:

```dotnet run load process```

Results will be provided in file like these:

| SYMBOL | NAME                       | MARKET_PRICE | DIV_PER_YEAR | DIV_YIELD | DIV_COUNT | FIRST_DIV_DATE | LAST_DIV_DATE | DIV_INCREASE_YEARS |
| ------ | -------------------------- | ------------ | ------------ | --------- | --------- | -------------- | ------------- | ------------------ |
| XOM    | Exxon Mobil Corporation    |        60.77 | 3.48         |    5.73 % |       123 | 02/01/1991     | 05/01/2021    |              19.93 |
| AAPL   | Apple Inc.                 |       127.45 | 0.83         |    0.66 % |        56 | 02/01/1991     | 05/01/2021    |              30.27 |
| COO    | The Cooper Companies, Inc. |       389.19 | 0.06         |    0.02 % |        47 | 06/01/1999     | 01/01/2021    |               21.6 |
