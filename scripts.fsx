#!/usr/bin/env -S dotnet fsi
#r "nuget: FSharpMyExt, 2.0.0-prerelease.9"
#r "nuget: Twee.FSharp, 0.2.2"
open System
open System.IO
open FsharpMyExtension
open FsharpMyExtension.IO
open FsharpMyExtension.Containers

let (</>) x y = Path.Combine(x, y)

let gamePath = "src" </> "game.twee"
let newlineType = Twee.FSharp.NewlineType.Lf

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

let updateTwee update =
  Result.builder {
    let! twee = Twee.FSharp.Document.parseFile gamePath
    let twee = update twee
    let rawTwee = Twee.FSharp.Document.toString newlineType twee
    let rawTwee = $"{rawTwee}{Twee.FSharp.NewlineType.toString newlineType}"
    IO.File.WriteAllText(gamePath, rawTwee)
    return ()
  }

let addImage imageName passageName =
  updateTwee (fun twee ->
    let imagePath = @"src" </> "images" </> imageName + ".webp"
    let imageCssRule = ImageCssRule.createFromFile imagePath
    let twee = appendStyleRuleToStylesheet imageCssRule twee
    let twee =
      twee
      |> useImageInTopPassage
        (fun passage -> passage.Header.Name = passageName)
        imageCssRule
    twee
  )
  |> printfn "%A"

// ImageMagick.convertFolderToWebp false "src/images"

let addImages () =
  addImage "pentagram" "Спуститься за старушкой" // 1769298905
  addImage "1769360965" "Сбежать от мужичка"
  addImage "1769456633" "Искать бабулю"
  addImage "1769477722" "Наблюдать за бабулей"
  addImage "1769547712" "Отправиться в лес"
  addImage "1769555303" "Пойти за мужичком"
  addImage "1769626842" "Посмотреть, что там"
  addImage "1769627225" "Принести дров"
  addImage "1769709272" "Найти церковь"
  addImage "1769724226" "Сесть на ближайшую могилу"
  addImage "1769793840" "Дождаться ночи"
  addImage "1769797475" "Отправиться домой"
  addImage "1769801961" "Войти внутрь дома и Зайти"
  addImage "1769882666" "Присмотреться"
  addImage "bucket-with-potatoes" "Продрать очи" // 1769706858


module Achievements =
  open FsharpMyExtension.Serialization.Serializers.ShowList

  type Achievement = {
    Name: string
    Description: string option
    GitHubImagePath: string
    ImagePath: string
    PassageName: string
  }

  [<RequireQualifiedAccess>]
  [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
  module Achievement =
    let create achievementName passageName achievementGitHubImagePath achievementImagePath : Achievement =
      {
        Name = achievementName
        PassageName = passageName
        GitHubImagePath = achievementGitHubImagePath
        ImagePath = achievementImagePath
        Description = None
      }

  let showTab = showString "  "

  let showField key value =
    key << showChar ':' << showSpace << value << showChar ','

  let showQuotes =
    between (showChar '"') (showChar '"')

  let showBlock shows : ShowS list =
    shows
    |> List.map (fun (s: ShowS) ->
      showTab << s
    )

  let createDefaultAchievements (achievements: Achievement list) =
    let createField achievementName =
      showField
        (showQuotes (showString achievementName))
        (showString "true")
    [
      showString "const defaultAchievements = () => ({"
      yield!
        achievements
        |> List.map (fun achievement ->
          showTab << createField achievement.Name
        )
      showString "})"
    ]

  let createAchievementDescriptions (achievements: Achievement list) =
    let createField { Name = name; Description = description} =
      [
        showQuotes (showString name) << showString ": {"
        yield!
          [
            showField (showString "name") (showQuotes (showString name))
            showField (showString "desc") (showQuotes (showString (Option.defaultValue "" description)))
          ]
          |> showBlock
        showString "},"
      ]
    [
      showString "setup.achievementDescriptions = {"
      yield!
        achievements
        |> List.collect (fun achievementName ->
          createField achievementName
        )
        |> showBlock
      showString "}"
    ]

  let addInit achievements (twee: Twee.FSharp.Document) =
    twee
    |> Twee.FSharp.Document.updatePassage
      (fun passage ->
        passage.Header.Name = "StoryInit"
      )
      (fun passage ->
        { passage with
            Body =
              passage.Body
              |> List.collect (fun line ->
                if not <| line.Contains "// todo: добавить инициализацию достижений" then
                  [line]
                else
                  [
                    yield! createDefaultAchievements achievements |> showBlock |> List.map show
                    ""
                    yield! createAchievementDescriptions achievements |> showBlock |> List.map show
                  ]
              )
        }
      )

  let createUseTag { Name = achievementName } =
    String.concat "\n" [
      $"<<run setup.unlock(\"{achievementName}\")>>\\"
      $"Вы получили достижение <<link \"{achievementName}\">>"
      $"  <<run setup.showAchievement(\"{achievementName}\")>>"
      "<</link>>!"
    ]

  let achievements =
    [
      Achievement.create "Есть контакт"      "Сдаться чарам девушки"        "151ea21b-d705-4e94-aef4-01cafd683c1f" "549112654-151ea21b-d705-4e94-aef4-01cafd683c1f"
      Achievement.create "Крушитель"         "Выломать дверь"               "f506f819-c35e-45c9-a6df-e2dcf330f11a" "549112688-f506f819-c35e-45c9-a6df-e2dcf330f11a"
      Achievement.create "Спасатель курочки" "Отпустить курицу"             "17991d3a-b2d2-4ec5-9c86-0986a06b9a49" "549112727-17991d3a-b2d2-4ec5-9c86-0986a06b9a49"
      Achievement.create "Первая кровь"      "Выпить кровь курицы"          "31a415d8-434f-4ab7-9094-1afac56a5595" "549112826-31a415d8-434f-4ab7-9094-1afac56a5595"
      Achievement.create "Спасатель зайчика" "Освободить зайчика и убежать" "cbcac3f6-2d65-4d43-9589-22dd71c6e35b" "549112883-cbcac3f6-2d65-4d43-9589-22dd71c6e35b"
      Achievement.create "Исцеление"         "Концовка исцеление"           "287c9124-4b44-4048-8498-fe3cc527294d" "549112925-287c9124-4b44-4048-8498-fe3cc527294d"
      Achievement.create "Монстр"            "Концовка монстр"              "d1b89dc9-f03f-4200-8822-6d3ffda7116e" "549112983-d1b89dc9-f03f-4200-8822-6d3ffda7116e"
      Achievement.create "Псих"              "Концовка псих"                "5d2042c6-a526-40d2-9f8b-16ed9e620cf2" "549113060-5d2042c6-a526-40d2-9f8b-16ed9e620cf2"
    ]

  // do
  //   updateTwee (fun twee ->
  //     addInit achievements twee
  //   )
  //   |> printfn "%A"


  // ImageMagick.convertFolderToWebp false "src/achievement-images"
