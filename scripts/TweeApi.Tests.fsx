#!/usr/bin/env -S dotnet fsi
#r "nuget: Expecto, 10.2.3"
#load "TweeApi.fsx"
open FsharpMyExtension.Serialization.Deserializers
open FsharpMyExtension.Serialization.Serializers
open Expecto

open TweeApi

[<Tests>]
let ``PassageName.Parser.parser`` =
    let parser = PassageName.Parser.parser
    testList "PassageName.Parser.parser" [
        testCase "1" <| fun () ->
            Expect.equal
                (FParsec.runResult parser (
                    String.concat "\n" [
                        ":: Passage"
                    ]
                ))
                (Ok "Passage")
                ""
    ]

[<Tests>]
let ``PassageName.Printer.shows`` =
    let shows = PassageName.Printer.shows
    testList "PassageName.Printer.shows" [
        testCase "1" <| fun () ->
            Expect.equal
                (ShowList.show <| shows "Passage")
                ":: Passage"
                ""
    ]

[<Tests>]
let ``PassageBody.Parser.parser`` =
    let parser = PassageBody.Parser.parser
    testList "PassageBody.Parser.parser" [
        testCase "1" <| fun () ->
            Expect.equal
                (FParsec.runResult parser (
                    String.concat "\n" [
                        "lorem"
                        ""
                        "ipsum dei"
                        ""
                        ":: Next passage"
                    ]
                ))
                (Ok [
                    "lorem"
                    ""
                    "ipsum dei"
                    ""
                ])
                ""
    ]

[<Tests>]
let ``PassageBody.Printer.shows`` =
    let shows = PassageBody.Printer.shows
    testList "PassageBody.Printer.shows" [
        testCase "1" <| fun () ->
            Expect.equal
                (ShowList.show <| shows NewlineType.Lf [
                    "lorem"
                    ""
                    "ipsum dei"
                    ""
                ])
                (String.concat "\n" [
                    "lorem"
                    ""
                    "ipsum dei"
                    ""
                ])
                ""
    ]

runTestsWithCLIArgs
    []
    fsi.CommandLineArgs[1..]
    (testList "all" [
        ``PassageName.Parser.parser``
        ``PassageName.Printer.shows``
        ``PassageBody.Parser.parser``
        ``PassageBody.Printer.shows``
    ])
