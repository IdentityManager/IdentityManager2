# IdentityManager2

[![NuGet](https://img.shields.io/nuget/vpre/IdentityManager2.svg)](https://www.nuget.org/packages/IdentityManager2) [![Gitter](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/IdentityManager/IdentityManager?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

IdentityManager2 is a tool for developers and/or administrators to manage the identity information for users of their applications in ASP.NET Core. This includes creating users, editing user information (passwords, email, claims, etc.) and deleting users. It provides a modern replacement for the ASP.NET WebSite Administration tool that used to be built into Visual Studio.

In theory, IdentityManager2 can work with any user store, it just requires an implementation of `IIdentityManagerService`. For example ASP.NET Core Identity usage, check out [IdentityManager2.AspNetIdentity](https://github.com/IdentityManager/IdentityManager2.AspNetIdentity).

IdentityManager2 is a development tool and is not designed to be used in production. For production identity management see [AdminUI](https://www.identityserver.com/products).

## Articles

- [Getting Started with IdentityManager2](https://www.scottbrady91.com/ASPNET-Identity/Getting-Started-with-IdentityManager2)
- [IdentityManager2 2020 Update](https://www.scottbrady91.com/ASPNET-Identity/IdentityManager2-2020-Update)

## Contributing

Currently, IdentityManager2 is a port of the original IdentityManager dev tool. If you're interested in helping to update the codebase, then check out the [issue tracker](https://github.com/IdentityManager/IdentityManager2/issues?q=label%3A%22help+wanted%22+is%3Aissue+is%3Aopen).

Developed and maintained by [Rock Solid Knowledge](https://www.identityserver.com).
