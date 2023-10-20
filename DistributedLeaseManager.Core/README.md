# DistributedLeaseManager.Core

[![NuGet](https://img.shields.io/nuget/v/DistributedLeaseManager.Core)](https://nuget.org/packages/DistributedLeaseManager.Core)

## Description

This library contains the core files of the simple C#/.NET [distributed lease/lock manager (DLM)](https://en.wikipedia.org/wiki/Distributed_lock_manager) implementation.

Use this if you want to implement a custom lease storage; otherwise, use an existing implementation library, e.g. `DistributedLeaseManager.AzureBlobStorage`.

Inspired by the https://github.com/fbeltrao/azfunctions-distributed-locking

## Contributing / Implementing a custom lease storage

If you'd like to use a lease storage different from the ones provided by the author, add the `DistributedLeaseManager.Core` library to your project and implement the corresponding interface (see any of the existing implementations as an example).

Feel free to open a PR with your changes to include them in the package!
