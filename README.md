Project Goals
==============
-To develop a Twitter bot that interacts with the Darwin API (through  Huxley: A JSON rest proxy for the Darwin SOAP API). The final code should be able to accurately show and tweet the cancelled train journeys to and from Liverpool Lime Street. 

-The 'TweetHandler' model should be coded in such a way for wide adaptability. So it can be easily imported to other applications such as one using the ASP.NET framework rather the WPF.

Improvements That Could Be Applied
==================================
-Reduce code redundancy, especially in the TweetHandler model.

-The code could be further expanded to include many more stations. This would require a rework of the code which populates the list of active train services in the TwitterHandler model.

User Notes
==========

(14/05/2018):
You now have to set the Twitter API keys in the UI. The program can also send out a 'test' tweet to verify that the credentials are fine by clicking the 'send test tweet' button on the toolbar.

Acknowledgements 
================

James Singleton for the development of the Huxley API. Which is a fantastic open source RESTful proxy that returns easy to manipulate JSON data from the National Rail Live Departures Board API. 
Link: https://huxley.apphb.com/
