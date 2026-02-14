#!/usr/bin/env -S dotnet fsi
#r "nuget: FSharpMyExt, 2.0.0-prerelease.9"
#r "nuget: Twee.FSharp, 0.2.2"
open System
open System.IO
open FsharpMyExtension
open FsharpMyExtension.IO
open FsharpMyExtension.Containers

let (</>) x y = Path.Combine(x, y)

module ImageMagick =
  let convert src dst =
    let convertPath =
      match Environment.OSVersion.Platform with
      | PlatformID.Unix -> "convert"
      | _ -> @"e:\msys64\mingw64\bin\convert.exe"
    Proc.startProcSimple convertPath (
      [$"\"{src}\""; $"\"{dst}\""]
      |> String.concat " "
    )

  let convertFolderToWebp dry dirPath =
    let dir = DirectoryInfo dirPath
    let files = dir.GetFiles "*.png"
    files
    |> Array.iter (fun file ->
      let srcPath = file.FullName
      let dstPath =
        file.DirectoryName </> $"{Path.GetFileNameWithoutExtension srcPath}.webp"
      printfn "convert %s %s" srcPath dstPath
      if not dry then
        convert srcPath dstPath
        |> printfn "%d"
    )

type ImageCssRule = {
  Name: string
  Body: string
}
[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module ImageCssRule =
  let createFromFile path =
    let imageBase64 =
      File.ReadAllBytes path
      |> System.Convert.ToBase64String
    let filename = Path.GetFileNameWithoutExtension path
    let name = $"img-{filename}"
    let body =
      String.concat "\n" [
        $".{name} {{"
        "  width: 155px;"
        "  height: 277px;"
        $"  background-image: url(\"data:image/webp;base64,{imageBase64}\");"
        "  background-size: contain;"
        "  background-repeat: no-repeat;"
        "  display: inline-block;"
        "}"
      ]
    {
      Name = name
      Body = body
    }

  let createUseHtmlTag (imageStyle: ImageCssRule) =
    $"<div class=\"{imageStyle.Name} float-right\" />"

let appendStyleRuleToStylesheet (styleRule: ImageCssRule) (twee: Twee.FSharp.Document) =
  twee
  |> Twee.FSharp.Document.updatePassage
    (fun passage ->
      let header = passage.Header
      match header.Tags with
      | None -> false
      | Some tags ->
        header.Name = "StoryStylesheet" && Set.contains "stylesheet" tags
    )
    (fun passage ->
      { passage with
          Body =
            List.append passage.Body [styleRule.Body]
      }
    )

let useImageInTopPassage passageName (styleRule: ImageCssRule) twee =
  twee
  |> Twee.FSharp.Document.updatePassage passageName (fun passage ->
    { passage with
        Body =
          let htmlTag = ImageCssRule.createUseHtmlTag styleRule
          $"{htmlTag}\\"::passage.Body
    }
  )

let f imagePath passageName =
  let gamePath = "src" </> "game.twee"
  Result.builder {
    let imageCssRule = ImageCssRule.createFromFile imagePath
    let! twee = Twee.FSharp.Document.parseFile gamePath
    let twee = appendStyleRuleToStylesheet imageCssRule twee
    let twee = useImageInTopPassage passageName imageCssRule twee
    let rawTwee = Twee.FSharp.Document.toString Twee.FSharp.NewlineType.Lf twee
    IO.File.WriteAllText(gamePath, rawTwee)
    return ()
  }

// ImageMagick.convertFolderToWebp false "src/images"
f
  @"src/images/1769360965.webp"
  (fun passage -> passage.Header.Name = "Сбежать от мужичка")
|> printfn "%A"
