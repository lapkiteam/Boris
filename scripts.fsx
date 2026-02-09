#!/usr/bin/env -S dotnet fsi
#r "nuget: FSharpMyExt, 2.0.0-prerelease.9"
open System.IO
open FsharpMyExtension
open FsharpMyExtension.IO

let (</>) x y = Path.Combine(x, y)

let convert src dst =
  let convertPath = @"e:\msys64\mingw64\bin\convert.exe"
  Proc.startProcSimple convertPath (
    [$"\"{src}\""; $"\"{dst}\""]
    |> String.concat " "
  )

let convertToWebp () =
  let dry = false
  let dirPath = "src/images"
  let dir = DirectoryInfo dirPath
  let files = dir.GetFiles("*.png")
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

// convertToWebp()
f @"src\images\bucket-with-potatoes.webp"
