namespace FSharpEcommerce.Extensions

open Microsoft.AspNetCore.Builder
open System.Runtime.CompilerServices
open System.Reflection
open System

type IEndpointMapper =
    abstract member Map: WebApplication -> unit

type EndpointsExtension =
    [<Extension>]
    static member MapEndpoints(app: WebApplication) =
        // find all types that implement IEndpointMapper through reflection
        let endpointMappers =
            Assembly.GetExecutingAssembly().GetTypes()
            |> Array.filter (fun t -> t.IsClass && not t.IsAbstract && t.GetInterface("IEndpointMapper") <> null)
            |> Array.map (fun t -> Activator.CreateInstance t :?> IEndpointMapper)

        endpointMappers |> Array.iter (fun mapper -> mapper.Map app)
