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

let toBase64 path =
  let base64 =
    File.ReadAllBytes path
    |> System.Convert.ToBase64String
  let name = Path.GetFileNameWithoutExtension path
  let styleName = $"img-{name}"
  let style =
    String.concat "\n" [
      $".{styleName} {{"
      "  width: 155px;"
      "  height: 277px;"
      $"  background-image: url(\"data:image/webp;base64,{base64}\");"
      "  background-size: contain;"
      "  background-repeat: no-repeat;"
      "  display: inline-block;"
      "}"
    ]
  let tag =
    $"<div class=\"{styleName} float-right\" />\\"
  String.concat "\n" [
    style
    tag
  ]
  |> Clipboard.setText

// convertToWebp()
toBase64 @"src\images\1769801961.webp"
