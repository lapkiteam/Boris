// @ts-check

const ArrayExt = {
  /**
   * @template T
   * @template U
   * @param {T[]} arr
   * @param {(item: T) => U} projection
   * @returns {T[]}
   */
  getFirstDuplicates(arr, projection) {
    if (arr.length === 0) {
      throw new Error('Array is empty!')
    }

    const first = projection(arr[0])
    const result = []

    for (const item of arr) {
      if (projection(item) === first) {
        result.push(item)
      } else {
        break
      }
    }
    return result
  }
}

/** @typedef {"healing" | "psycho" | "monster"} CharacteristicName */

const CharacteristicName = {
  /**
   * @param {CharacteristicName} name
   */
  getOrder(name) {
    switch (name) {
      case "monster": return 3 // монстром легче всего стать
      case "psycho": return 2
      case "healing": return 1 // вылечиться сложнее всего
      default: return 0
    }
  }
}

/** @typedef {{ name: CharacteristicName, value: number }} Characteristic */

/**
 * @param {Characteristic[]} characteristics
 */
function getTopCharacteristic(characteristics) {
  if (!Array.isArray(characteristics) || characteristics.length === 0) {
    throw new Error('Array is empty!')
  }

  const byValueDesc = [...characteristics].sort((a, b) => b.value - a.value)
  const topValueGroup = ArrayExt.getFirstDuplicates(byValueDesc, x => x.value)
  const byOrderDesc = topValueGroup.sort(
    (a, b) => CharacteristicName.getOrder(b.name) - CharacteristicName.getOrder(a.name)
  )
  return byOrderDesc[0]
}

function testGetTopCharacteristic() {
  {
    const result = getTopCharacteristic([
      { name: "healing", value: 1 },
      { name: "monster", value: 3 },
      { name: "psycho", value: 2 },
    ])
    console.log(result.name === "monster" && result.value === 3)
  }

  {
    const result = getTopCharacteristic([
      { name: "healing", value: 0 },
      { name: "psycho", value: 2 },
      { name: "monster", value: 2 },
    ])
    console.log(result.name === "monster" && result.value === 2)
  }

  {
    const result = getTopCharacteristic([
      { name: "healing", value: 2 },
      { name: "psycho", value: 2 },
      { name: "monster", value: 1 },
    ])
    console.log(result.name === "psycho" && result.value === 2)
  }
}

// testGetTopCharacteristic()

window["getTopCharacteristic"] = getTopCharacteristic
