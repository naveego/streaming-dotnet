# Naveego Streaming Library

![Build Status](https://ci.n5o.black/app/rest/builds/buildType:(id:SharedLibraries_StreamingDotnet_Build)/statusIcon)

This is a library for writing streaming applications on dotnet core.  It creates a generic
streaming abstraction that is easy to program against, and then provides implementations for 
common streaming systems such as [Apache Kafka]. (Ok...Well only Kafka at the moment).

## Getting Started

To get started install the library using NuGet.

#### Package Manager
```
Install-Package Naveego.Streaming.Kafka
```

#### .NET CLI
```
dotnet add package Naveego.Streaming.Kafka
```


## Async

The library is written with Async in mind.  If you are writing streaming applications and not using 
asynchronous patterns, then you probably should be!


## Reading from a stream

The `IStreamReader<>` interface provides the API for reading streaming messages in your application.

```c#
public interface IStreamReader<out T>
{
    Task ReadAsync(Func<T, Task<HandleResult>> onMessage, CancellationToken cancellationToken);
}
```

## Writing to a stream

The `IStreamWriter<>` interface provides the API for writing streaming messages in your application.

```c#
 public interface IStreamWriter<in T>
 {
    Task WriteAsync(T record);
 }    
```

## HandleResult

Writing streaming applications can be very complicated, especially when things go wrong.  Our goal with 
this library was to handle as much heaving lifting as we could.  There are two primary areas where things can 
go wrong:

  - *Bad Messages* are problems with the processing of the payload themselves.  This means the underlying streaming
     system is functioning properly, but the message itself has something wrong with it and is not able to be processed.
     
  - *Streaming System Errors* are problems with the underlying streaming system.  These errors either indicate a temporary
     condition (like a network issue), or are critical (like a misconfiguration or invalid permissions). 

The `HandleResult` class allows the author to tell if the message processing was successful, or if there was a 
temporary/recoverable issue.  It has an indicator that identifies success and a `IRetryStrategy` that tells the library
how to respond to the temporary errors.

```c#
 public class HandleResult
 {
    public readonly IRetryStrategy RetryStrategy;
    public readonly bool Success;
 }
``` 

## Kafka

We provide a [Apache Kafka] implementation out of the box.  Here is an example of using the `KafkaStreamReader<>`.

```c#
public class MyKafkaApplication
{
    private readonly IStreamReader<Customer> _streamReader;

    public MyKafkaApplication(IStreamReader<Customer> streamReader)
    {
        _streamReader = streamReader;
    }

    public async Task Run()
    {
        await _streamReader.ReadAsync(Persist);
    }

    private async Task<HandleResult> ProcessCustomer(Customer customer)
    {
        try
        {
            await DoSomethingWithTheCustomerAsync(customer);
            return HandleResult.Ok;
        }
        catch (Exception ex)
        {
            return HandleResult.Bad;
        }
    }
}
```


[Apache Kafka]: https://kafka.apache.org/

