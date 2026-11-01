// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
open System
open System.Numerics
open System.Runtime.InteropServices
open MathemaSharp

let bits<'T when 'T : struct and 'T :> ValueType and 'T : (new : unit -> 'T)> (value: 'T) =
    Convert.ToHexString(MemoryMarshal.AsBytes<'T>(ReadOnlySpan<'T>([| value |])))

let emit<'T when 'T : struct and 'T :> ValueType and 'T : (new : unit -> 'T) and 'T :> IFloatingPointIeee754<'T>> () =
    let format = typeof<'T>.Name
    let operations: (string * ('T -> 'T)) list =
        [ "Sin", (fun x -> GenericMath.Sin<'T>(x))
          "Cos", (fun x -> GenericMath.Cos<'T>(x))
          "Tan", (fun x -> GenericMath.Tan<'T>(x))
          "Sqrt", (fun x -> GenericMath.Sqrt<'T>(x))
          "Exp", (fun x -> GenericMath.Exp<'T>(x))
          "Log", (fun x -> GenericMath.Log<'T>(x)) ]
    for definition in MathematicalConstants.Definitions do
        let value = definition.Value<'T>()
        printfn "math|%s|%s|%s" format definition.Id (bits value)
        for name, operation in operations do
            printfn "scalar|%s|%s|%s|%s" format definition.Id name (bits (operation value))
    for definition in PhysicalConstants.Definitions do
        try
            let value = PhysicalConstants.Get<'T>(definition.Id)
            printfn "physical|%s|%s|%s|%s" format definition.Id (bits value.Value) (bits value.StandardUncertainty)
        with
        | :? ArithmeticException as error ->
            printfn "physical-error|%s|%s|%s" format definition.Id (error.GetType().Name)

[<EntryPoint>]
let main _ =
    emit<Half>()
    emit<single>()
    emit<double>()
    emit<Decimal32>()
    emit<Decimal64>()
    emit<Decimal128>()
    0
