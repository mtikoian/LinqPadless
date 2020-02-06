# Script Tests


## Plain Expression

Suppose:

- kind is expression

Source:

```
DateTime.Now
```

Expected:

```
<Query Kind="Expression" />

DateTime.Now
```


## One import

Suppose:

- kind is expression

Source:

```
using System.Net.Http;

DateTime.Now
```

Expected:

```
<Query Kind="Expression">
  <Namespace>System.Net.Http</Namespace>
</Query>

DateTime.Now
```


## Several imports

Suppose:

- kind is expression

Source:

```
using System.Net.Http;
using System.Security.Cryptography;

DateTime.Now
```

Expected:

```
<Query Kind="Expression">
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

DateTime.Now
```


## Mixed types of imports

Suppose:

- kind is expression

Source:

```
using System.Net.Http;
using System.Security.Cryptography;
using static System.Math;
using Int = System.Int32;

DateTime.Now
```

Expected:

```
<Query Kind="Expression">
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>Int = System.Int32</Namespace>
</Query>

DateTime.Now
```


## Imports without semi-colon termination

Suppose:

- kind is expression

Source:

```
using System.Net.Http
using System.Security.Cryptography
using static System.Math
using Int = System.Int32
DateTime.Now
```

Expected:

```
<Query Kind="Expression">
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>Int = System.Int32</Namespace>
</Query>

DateTime.Now
```


## Spaced-out imports

Suppose:

- kind is expression

Source:

```
using System.Net.Http

using System.Security.Cryptography

using static System.Math

using Int = System.Int32

DateTime.Now
```

Expected:

```
<Query Kind="Expression">
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>Int = System.Int32</Namespace>
</Query>

DateTime.Now
```


## Commented-out imports

Suppose:

- kind is expression

Source:

```
using System.Net.Http
//using System.Security.Cryptography
using static System.Math
//using Int = System.Int32

DateTime.Now
```

Expected:

```
<Query Kind="Expression">
  <Namespace>System.Net.Http</Namespace>
  <Namespace>static System.Math</Namespace>
</Query>

//using System.Security.Cryptography
//using Int = System.Int32

DateTime.Now
```


## NuGet references

Suppose:

- kind is expression

Source:

```
#r "nuget: System.Reactive"
using System.Reactive.Linq;
			
Observable.Range(1,10)
```

Expected:

```
<Query Kind="Expression">
  <NuGetReference>Rx-Main</NuGetReference>
  <Namespace>System.Reactive.Linq</Namespace>
</Query>

Observable.Range(1,10)
```
