// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


/**
 * Demonstrates something
 */


const resolver = require('../../out/resolver.js')


const repositoryEndpoint = 'devicemodels.azure.com'
const dtmi = process.argv[2] || 'dtmi:azure:DeviceManagement:DeviceInformation;1'

console.log(repositoryEndpoint, dtmi)

async function main() {

  result = await resolver.resolve(dtmi, repositoryEndpoint);
  console.log(result)
}

main().catch((err) => {
    console.error("The sample encountered an error:", err);
  });