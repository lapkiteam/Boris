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

/** @typedef {"adequacy" | "inadequacy" | "capitalism" | "tlenost"} CharacteristicName */

const CharacteristicName = {
  /**
   * @param {CharacteristicName} name
   */
  getOrder(name) {
    switch (name) {
      case "adequacy": return 4
      case "inadequacy": return 3
      case "capitalism": return 2
      case "tlenost": return 1
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
      { name: "adequacy", value: 1 },
      { name: "tlenost", value: 4 },
      { name: "capitalism", value: 3 },
      { name: "inadequacy", value: 2 },
    ])
    console.log(result.name === "tlenost" && result.value === 4)
  }

  {
    const result = getTopCharacteristic([
      { name: "adequacy", value: 0 },
      { name: "inadequacy", value: 2 },
      { name: "capitalism", value: 2 },
      { name: "tlenost", value: 1 },
    ])
    console.log(result.name === "inadequacy" && result.value === 2)
  }

  {
    const result = getTopCharacteristic([
      { name: "adequacy", value: 2 },
      { name: "inadequacy", value: 2 },
      { name: "capitalism", value: 1 },
      { name: "tlenost", value: 2 },
    ])
    console.log(result.name === "adequacy" && result.value === 2)
  }
}

// testGetTopCharacteristic()

window["getTopCharacteristic"] = getTopCharacteristic
