# Markdown file

Maximum number of PropertyIds for any one given Object.
private const int LOG_BASE2_MAX_PROPERTIES = 16;
public static readonly int MAX_NUMBER_OF_PROPERTIES = (int)Math.Pow(2, LOG_BASE2_MAX_PROPERTIES); //65536;

Note: This leaves 2 ^^ 48 possible ObjectIds. (2 ^^ 48 = 280 trillon (281,474,976,710,656)

Consider making the composite key include an event subscription identifer.
If we allow 4096 properties for each object and 4096 subscriptions for each property.

2 ^^ 12 + 2 ^^ 12, leaves 2 ^^ (64 - 24) or 2 ^^ 40 or 1,099,511,627,776 or more than 1 trillion objects per life time of an application domain.

If we cap the max # of props to 1024 and cap the max # of subscriptions to 256, then this leaves 64 - (10 + 8) or 46 bits for the objectId. 
2 ^^ 46 = 70.3 trillion.