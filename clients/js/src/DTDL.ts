// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

'use strict'

export interface DTDL extends JSON {
  '@context': any[],
  '@id': string,
  'extends': string,
  'contents': Contents[]
}

interface Contents {
  '@type'?: string;
  'name': string;
  'schema': string;
}