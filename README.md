## Primary Constructor

Generate primary constructor from readonly fields

[![Actions](https://github.com/wk-j/primary-constructor/workflows/NuGet/badge.svg)](https://github.com/wk-j/primary-constructor/actions)
[![NuGet](https://img.shields.io/nuget/v/wk.PrimaryConstructor.svg)](https://www.nuget.org/packages/wk.PrimaryConstructor)
[![NuGet Downloads](https://img.shields.io/nuget/dt/wk.PrimaryConstructor.svg)](https://www.nuget.org/packages/wk.PrimaryConstructor)

![image](https://user-images.githubusercontent.com/5763993/97197488-4b65ad80-17e0-11eb-9eef-305ce284eb78.png)

### Prerequisites

Visual Studio version 16.8 and above is required as its first version to support source generators.

### Usage

Declare class with `partial`, and annotate with `[PrimaryConstructor]`.
And then you can declare your dependencies with readonly fields.

```csharp
[PrimaryConstructor]
public partial class MyService
{
    private readonly MyDependency _myDependency;

    ...
}
```

When compile, following source will be injected.

```csharp
partial class MyService
{
    public MyService(MyDependency myDependency)
    {
        this._myDependency = myDependency;
    }
}
```
