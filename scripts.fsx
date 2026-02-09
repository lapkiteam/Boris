#!/usr/bin/env -S dotnet fsi
#r "nuget: FSharpMyExt, 2.0.0-prerelease.9"
open System
open System.IO
open FsharpMyExtension
open FsharpMyExtension.IO

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
    $"<div class=\"{imageStyle.Name} float-right\" />\\"

let f imagePath =
  let imageCssRule = ImageCssRule.createFromFile imagePath
  String.concat "\n" [
    imageCssRule.Body
    ImageCssRule.createUseHtmlTag imageCssRule
  ]
  |> Clipboard.setText

// ImageMagick.convertFolderToWebp false "src/images"
f @"src/images/bucket-with-potatoes.webp"
