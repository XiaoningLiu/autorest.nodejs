/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for
 * license information.
 *
 * Code generated by Microsoft (R) AutoRest Code Generator.
 * Changes may cause incorrect behavior and will be lost if the code is
 * regenerated.
 */

'use strict';

/**
 * Class representing a ProductProperties.
 */
class ProductProperties {
  /**
   * Create a ProductProperties.
   * @member {number} [id]
   * @member {string} [name]
   */
  constructor() {
  }

  /**
   * Defines the metadata of ProductProperties
   *
   * @returns {object} metadata of ProductProperties
   *
   */
  mapper() {
    return {
      required: false,
      serializedName: 'Product_properties',
      type: {
        name: 'Composite',
        className: 'ProductProperties',
        modelProperties: {
          id: {
            required: false,
            serializedName: 'id',
            type: {
              name: 'Number'
            }
          },
          name: {
            required: false,
            serializedName: 'name',
            type: {
              name: 'String'
            }
          }
        }
      }
    };
  }
}

module.exports = ProductProperties;