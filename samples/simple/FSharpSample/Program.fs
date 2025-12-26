open CSnakes.Runtime
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open System
open System.IO
open System.Collections.Generic

let quickDemo (env: IPythonEnvironment) =
    let qd = env.QuickDemo()
    qd.Scream("a", 99) |> printfn "Scream: %A"
    qd.ScreamNames(["a"; "b"; "c"], 3) |> printfn "ScreamNames: %A"

    env

let dictDemo (env: IPythonEnvironment) =
    let dd = env.TypeDemos()
    let d = dd.ReturnDict()

    // not ideal as it would be better if we could use Map and not have to
    // work with the KVPair type
    d |> Seq.iter (fun (x) -> printfn "Key: %A, Value: %A" x.Key x.Value)

    env

let kmeans (env: IPythonEnvironment) =
    let km = env.KmeansExample()

    // clunky here, as F# tuples are reference by default so we need to use the
    // struct keyword to make them value types (but only on the first, the rest
    // are inferred)
    let data = [struct (1L, 2L); (1L, 4L); (1L, 0L); (10L, 2L); (10L, 4L); (10L, 0L)]

    let struct (centroids, inertia) = km.CalculateKmeansInertia(data, 4)
    printfn "KMeans inertia for 4 clusters is %A, inertia is %A" centroids inertia

    env

let builder = Host.CreateDefaultBuilder().ConfigureServices(fun services ->
    let home = Path.Join(Environment.CurrentDirectory, "..", "..", "..", "..", "ExamplePythonDependency")
    let venv = Path.Join(home, ".venv")

    services
     .WithPython()
     .WithHome(home)
     .WithVirtualEnvironment(venv)
     .FromNuGet("3.12.4")
     .FromMacOSInstallerLocator("3.12")
     .FromEnvironmentVariable("Python3_ROOT_DIR", "3.12")
     .WithPipInstaller() |> ignore
)

let app = builder.Build()

app.Services.GetRequiredService<IPythonEnvironment>()
|> quickDemo
|> dictDemo
|> kmeans
|> ignore
