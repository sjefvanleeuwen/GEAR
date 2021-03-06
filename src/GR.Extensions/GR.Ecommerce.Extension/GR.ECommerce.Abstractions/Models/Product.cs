﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using GR.Core;
using GR.Core.Attributes;

namespace GR.ECommerce.Abstractions.Models
{
    public class Product : BaseModel
    {
        /// <summary>
        /// Product name
        /// </summary>
        [Required]
        [DisplayTranslate(Key = "name")]
        public virtual string Name { get; set; }
        /// <summary>
        /// Display name
        /// </summary>
        [Required]
        public virtual string DisplayName { get; set; }
        /// <summary>
        /// Short Description
        /// </summary>
        public virtual string ShortDescription { get; set; }
        /// <summary>
        /// Product Description
        /// </summary>
        [DisplayTranslate(Key = "description")]
        public virtual string Description { get; set; }
        /// <summary>
        /// Product Specification
        /// </summary>
        public string Specification { get; set; }
        /// <summary>
        /// Reference to brand
        /// </summary>
        public virtual Brand Brand { get; set; }
        public virtual Guid BrandId { get; set; }

        /// <summary>
        /// Reference to product type
        /// </summary>
        public virtual ProductType ProductType { get; set; }
        [Required]
        public virtual Guid ProductTypeId { get; set; }

        /// <summary>
        /// Icon image
        /// </summary>
        public virtual byte[] Thumbnail { get; set; }

        /// <summary>
        /// Product images
        /// </summary>
        public virtual IEnumerable<ProductImage> ProductImages { get; set; }

        /// <summary>
        /// Product attributes
        /// </summary>
        public virtual IEnumerable<ProductAttributes> ProductAttributes { get; set; }

        /// <summary>
        /// Product attributes
        /// </summary>
        public virtual IEnumerable<ProductVariation> ProductVariations { get; set; }

        /// <summary>
        /// Product prices
        /// </summary>
        public virtual IEnumerable<ProductPrice> ProductPrices { get; set; }

        /// <summary>
        /// Product categories
        /// </summary>
        public virtual IEnumerable<ProductCategory> ProductCategories { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ProductDiscount ProductDiscount { get; set; }

        /// <summary>
        /// Discount price
        /// </summary>
        public decimal DiscountPrice => (PriceWithoutDiscount * ProductDiscount?.Discount?.Percentage ?? 0) / 100;

        /// <summary>
        /// Current price
        /// </summary>
        public virtual decimal PriceWithoutDiscount => ProductPrices?.OrderBy(x => x.Created).LastOrDefault()?.Price ?? 0;

        /// <summary>
        /// Final price with discount
        /// </summary>
        public virtual decimal PriceWithDiscount => PriceWithoutDiscount - DiscountPrice;

        /// <summary>
        /// Publish state of product
        /// </summary>
        public virtual bool IsPublished { get; set; }

        /// <summary>
        /// Sku
        /// </summary>
        public virtual string Sku { get; set; }

        /// <summary>
        /// Gtin
        /// </summary>
        public virtual string Gtin { get; set; }
    }
}
