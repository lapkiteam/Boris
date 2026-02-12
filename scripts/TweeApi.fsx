#r "nuget: FParsec, 1.1.1"
#r "nuget: FSharpMyExt, 2.0.0-prerelease.9"

module CommonParser =
    open FParsec

    type 'a Parser = Parser<'a, unit>

[<RequireQualifiedAccess>]
type NewlineType =
    | Lf
    | CrLf

[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module NewlineType =
    let toString newlineType =
        match newlineType with
        | NewlineType.Lf -> "\n"
        | NewlineType.CrLf -> "\r\n"

type PassageName = string

[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module PassageName =
    module Parser =
        open FParsec

        open CommonParser

        let parser: string Parser =
            skipString "::" >>. spaces
            >>. manySatisfy ((<>) '\n')

    module Printer =
        open FsharpMyExtension.Serialization.Serializers.ShowList

        let shows (passageName: PassageName) =
            showString "::" << showSpace << showString passageName

type PassageBody = string list

[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module PassageBody =
    module Parser =
        open FParsec

        open CommonParser

        let parser: PassageBody Parser =
            let pline: _ Parser =
                notFollowedByString "::"
                >>? many1Satisfy ((<>) '\n')
            many (choice [
                pline .>> skipNewline
                newlineReturn ""
            ])

    module Printer =
        open FsharpMyExtension.Serialization.Serializers.ShowList

        let shows newlineType (passageBody: PassageBody) =
            let newline =
                showString <| NewlineType.toString newlineType
            passageBody
            |> List.map showString
            |> joinsEmpty newline

type Passage = {
    Name: PassageName
    Body: PassageBody
}

[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Passage =
    module Parser =
        open FParsec

        open CommonParser

        let parser: Passage Parser =
            pipe2
                PassageName.Parser.parser
                PassageBody.Parser.parser
                (fun name body ->
                    {
                        Name = name
                        Body = body
                    }
                )

    module Printer =
        let shows newlineType (passage: Passage) =
            PassageName.Printer.shows passage.Name
            << PassageBody.Printer.shows newlineType passage.Body

type Document = Passage list

[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Document =
    open FsharpMyExtension.Serialization.Deserializers
    open FsharpMyExtension.Serialization.Serializers

    module Parser =
        open FParsec

        open CommonParser

        let parser: Document Parser =
            many (Passage.Parser.parser .>> spaces)

    let parse (rawTwee: string) =
        FParsec.runResult Parser.parser rawTwee

    let parseFile (rawTwee: string) =
        FParsec.CharParsers.runParserOnFile
            Parser.parser
            ()
            rawTwee
            System.Text.Encoding.UTF8
        |> FParsec.ParserResult.toResult
        |> Result.map (fun (result, _, _) -> result)

    module Printer =
        open FsharpMyExtension.Serialization.Serializers.ShowList

        let shows newlineType (document: Document) =
            let newline =
                showString <| NewlineType.toString newlineType
            document
            |> List.map (Passage.Printer.shows newlineType)
            |> joinsEmpty newline

    let toString newlineType (document: Document) =
        Printer.shows newlineType document
        |> ShowList.show

    let updatePassage passageName update (twee: Document) =
        twee
        |> List.map (fun passage -> // todo: убедиться, что такой пассаж вообще существует
            if passage.Name <> passageName then passage
            else update passage
        )
