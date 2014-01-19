XmlDiff
=======

Overview
--------

Simple Xml diff tool based on Linq to Xml.
The main idea behind was to create a tool for a quick and flexible structured *.config files comparision.

Usage
-----

Basic usage is that you have two ```XElement``` instances ```sourceXml``` and ```resultXml```.
Then you can pass it to ```XmlComparer``` and inspect the result.
```
var comparer = new XmlComparer();
var diff = comparer.Compare(sourceXml, resultXml);
var isChanged = diff.IsChanged;
```

Feel free to use a predefined or custom Visitors
```
var visitor = new HtmlOutputVisitor();
visitor.Visit(diff, 0);
return visitor.Result;
```

Default output for ```diff.ToString()```  produces something like: 
```
= Element "configuration"
...= Element "applicationSettings"
......= Element "BusinessValues"
.........= Element "setting" "name"="IsProduction" "serializeAs"="String"
............= Element "value"
...............- Value: "False"
...............+ Value: "True"
...= Element "appSettings" "file"="Personal.config"
......= Element "add" "key"="webpages:Version" "value"="1.0.0.0"
.........+ Attribute: "value" with value: "1.0.0.0"
...= Element "system.web"
......- Element "authentication" "mode"="Forms"
```

License
-------

There is no any license information for now. Feel free to modify and use this code on you own. I will add a license once the project will become mature enough.