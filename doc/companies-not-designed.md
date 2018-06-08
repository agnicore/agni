# Why Companies Face Troubles 2 Years Down the Road?

Back to [Documentation Index](readme.md)

Companies/startups choose languages like Python/Ruby or PHP because those languages have a convenient set of libraries allowing them to put some working sites in production very quickly. The problem, however, is that later, when the number of users increases and business requirements start to demand more intense logic, those solutions either fail or become very hard/expensive to maintain for number of reasons: 

* Usually, a lack of proper architecture of the system as a whole. No consideration may have been given from day one to concerns like user geo affinity, distributed caching, session state management, common configuration formats, logging, instrumentation, management of large installations with many nodes. Developers usually do not consider: 

  * That any process (be it web server, app server, tool etc.) need to be remotely controlled in cluster environment so it can be stopped/queried/signaled
  * All tools must be command-line enabled (not UI only), so they can be scripted and executed in an unattended fashion
  *  There may be 100s of computers to configure, instead of 1. Are we ready to maintain 100s of configuration files?
  *  Time zones in distributed systems, cluster groups, NOCs. Where is time obtained from? What time zone? What time shows in global reports?
  *  Any UI element on the screen may be protected by permission (i.e. "Post Charges" button may not show for users who do not have access)
  *  Row-based security. Security checks may span not only screens/functions but also some data entities such as rows
  *  Web session state may not reside locally (i.e. local disk/memcache) if user reconnects to a different server in the cluster
  * Pushing messages to subscribers/topics. Using appropriate protocols (i.e. UDP). Not thought about when the whole system runs from one web server.

* Many systems use one central database instance (which is convenient to code against), and have big troubles when they need to split databases so they can scale, because all code depends on central data locations (e.g. one connect string used in 100s of classes)

* The scripting languages (e.g. PHP, Python, Ruby) used for web site implementation are not performant enough for solving general programming problems (try to build a PHP compiler in PHP) involved in high-throughput processing. It is slow for such tasks and was never meant to be used that way. What happens next, is that developers start to use C++ or C where the development paradigm is absolutely incompatible with the one in PHP, complexity keeps increasing as the number of standards internal to the system is increased. You need more personnel to develop this way

* Security functionality is usually overlooked as well as most applications do not have security beyond user login and conditional menu display on the homepage which depends on 5-10 fixed role checks. Later, businesses need to start protecting individual screens/UI elements with permissions. This usually creates mess in code and eventually precipitates a major re-write. * The inter-service security in the backend is usually either completely overlooked so any node can call any other node bypassing all checks, or just checks for one of two roles via a simple token.

* The ALM (Application LifeCycle Management) is usually not really though about. "We will deploy and manage changes somehow when we come to it"   




Back to [Documentation Index](readme.md)