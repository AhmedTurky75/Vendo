// ============================================================================
// EXAMPLE: Overcoming Primitive Obsession with Value Objects
// ============================================================================
//
// This example shows how to transform from using primitive types (string,
// decimal, int) to using value objects for domain concepts.
//
// KEY LEARNING:
// - Primitives don't convey domain meaning
// - Validation must be repeated everywhere
// - Value objects encapsulate validation and behavior
// - Impossible to create invalid value object
//
// ============================================================================

using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace Vendo.CatalogManagement.Examples
{
    // ╔═══════════════════════════════════════════════════════════════════════╗
    // ║                    BEFORE: PRIMITIVE OBSESSION                         ║
    // ║                         (Anti-Pattern)                                 ║
    // ╚═══════════════════════════════════════════════════════════════════════╝

    /// <summary>
    /// ❌ PRIMITIVE OBSESSION: Using primitives for domain concepts
    /// Problems:
    /// - No domain meaning (string SKU doesn't tell you format rules)
    /// - Validation scattered everywhere
    /// - Related data separated (price and currency)
    /// - Easy to make mistakes (swap parameters of same type)
    /// - No IDE help (all strings look the same)
    /// </summary>
    public class Product_Primitives
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        // ❌ Just a string - no validation, no format rules
        public string SKU { get; set; }

        // ❌ Price and currency separated - can get out of sync
        public decimal Price { get; set; }
        public string Currency { get; set; }

        // ❌ Multiple image URLs as separate properties - messy!
        public string ImageUrl1 { get; set; }
        public string ImageUrl2 { get; set; }
        public string ImageUrl3 { get; set; }

        // ❌ Dimensions as separate primitives
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public string DimensionUnit { get; set; }  // "cm", "in", etc.
    }

    /// <summary>
    /// ❌ SERVICE WITH SCATTERED VALIDATION
    /// Problems:
    /// - Validation logic duplicated across methods
    /// - Easy to forget validation
    /// - No central place for business rules
    /// </summary>
    public class ProductService_Primitives
    {
        public void CreateProduct(
            string name,
            string sku,
            decimal price,
            string currency)
        {
            // ❌ Validation must be repeated everywhere!
            if (string.IsNullOrWhiteSpace(sku))
                throw new ArgumentException("SKU is required");

            if (sku.Length < 3 || sku.Length > 50)
                throw new ArgumentException("SKU must be 3-50 characters");

            if (!Regex.IsMatch(sku, @"^[A-Z0-9\-_]+$"))
                throw new ArgumentException("SKU must be alphanumeric");

            if (price < 0)
                throw new ArgumentException("Price cannot be negative");

            if (string.IsNullOrEmpty(currency))
                throw new ArgumentException("Currency is required");

            if (currency.Length != 3)
                throw new ArgumentException("Currency must be 3-letter code");

            // Create product...
        }

        public void UpdatePrice(Guid productId, decimal price, string currency)
        {
            // ❌ Must validate again - duplication!
            if (price < 0)
                throw new ArgumentException("Price cannot be negative");

            if (string.IsNullOrEmpty(currency))
                throw new ArgumentException("Currency is required");

            if (currency.Length != 3)
                throw new ArgumentException("Currency must be 3-letter code");

            // Update...
        }

        public void AddProductImages(Guid productId, string url1, string url2, string url3)
        {
            // ❌ What if we need 4 images? Change method signature?
            // ❌ What if we only have 1 image? Pass null for others?

            if (!string.IsNullOrEmpty(url1) && !IsValidUrl(url1))
                throw new ArgumentException("Invalid URL format");

            if (!string.IsNullOrEmpty(url2) && !IsValidUrl(url2))
                throw new ArgumentException("Invalid URL format");

            if (!string.IsNullOrEmpty(url3) && !IsValidUrl(url3))
                throw new ArgumentException("Invalid URL format");

            // Update product...
        }

        public void SetDimensions(
            Guid productId,
            decimal length,
            decimal width,
            decimal height,
            string unit)
        {
            // ❌ Validation scattered
            if (length <= 0 || width <= 0 || height <= 0)
                throw new ArgumentException("Dimensions must be positive");

            if (unit != "cm" && unit != "in" && unit != "mm")
                throw new ArgumentException("Invalid dimension unit");

            // Update...
        }

        private bool IsValidUrl(string url) => true; // Simplified
    }

    // ╔═══════════════════════════════════════════════════════════════════════╗
    // ║                   AFTER: VALUE OBJECTS                                 ║
    // ║                     (Best Practice)                                    ║
    // ╚═══════════════════════════════════════════════════════════════════════╝

    // ────────────────────────────────────────────────────────────────────────
    // VALUE OBJECT 1: SKU (Stock Keeping Unit)
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// ✅ SKU VALUE OBJECT
    /// Benefits:
    /// - Validation centralized in one place
    /// - Format rules enforced at creation
    /// - Invalid SKU cannot exist (factory returns null)
    /// - Immutable - can't be changed after creation
    /// - Conveys domain meaning ("this is a SKU, not just any string")
    /// </summary>
    public sealed class SKU : IEquatable<SKU>
    {
        // ✅ Regex defined once, used everywhere
        private static readonly Regex SkuPattern =
            new Regex(@"^[A-Z0-9\-_]{3,50}$", RegexOptions.Compiled);

        // ✅ Immutable property
        public string Value { get; }

        // ✅ Private constructor - can't create directly
        private SKU(string value)
        {
            Value = value;
        }

        // ✅ Factory method with validation
        public static SKU? Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;

            var normalized = value.Trim().ToUpperInvariant();

            // ✅ Validation in one place
            if (!SkuPattern.IsMatch(normalized)) return null;

            return new SKU(normalized);
        }

        // ✅ Alternative factory for generating SKUs
        public static SKU Generate(string prefix)
        {
            var uniquePart = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
            return new SKU($"{prefix}-{uniquePart}");
        }

        // ✅ Value equality
        public bool Equals(SKU? other)
        {
            if (other is null) return false;
            return Value == other.Value;
        }

        public override bool Equals(object? obj) => Equals(obj as SKU);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value;

        public static bool operator ==(SKU? left, SKU? right) =>
            left?.Equals(right) ?? right is null;

        public static bool operator !=(SKU? left, SKU? right) =>
            !(left == right);
    }

    // ────────────────────────────────────────────────────────────────────────
    // VALUE OBJECT 2: Money
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// ✅ MONEY VALUE OBJECT
    /// Benefits:
    /// - Amount and currency always together (consistency)
    /// - Validation centralized
    /// - Domain operations built-in (Add, Subtract, etc.)
    /// - Can't accidentally swap amount and currency
    /// </summary>
    public sealed class Money : IEquatable<Money>
    {
        // ✅ Related data together
        public decimal Amount { get; }
        public string Currency { get; }

        // ✅ Private constructor
        private Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        // ✅ Factory with validation
        public static Money? Create(decimal amount, string currency = "USD")
        {
            // ✅ Validation centralized
            if (amount < 0) return null;
            if (string.IsNullOrWhiteSpace(currency)) return null;
            if (currency.Length != 3) return null;

            return new Money(amount, currency.ToUpperInvariant());
        }

        // ✅ Domain operations
        public Money Add(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException(
                    "Cannot add money with different currencies");

            return new Money(Amount + other.Amount, Currency);
        }

        public Money Subtract(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException(
                    "Cannot subtract money with different currencies");

            var newAmount = Amount - other.Amount;
            if (newAmount < 0)
                throw new InvalidOperationException(
                    "Result cannot be negative");

            return new Money(newAmount, Currency);
        }

        public Money MultiplyBy(decimal factor)
        {
            if (factor < 0)
                throw new ArgumentException("Factor cannot be negative");

            return new Money(Amount * factor, Currency);
        }

        public bool IsGreaterThan(Money other) => Amount > other.Amount;
        public bool IsLessThan(Money other) => Amount < other.Amount;

        // ✅ Value equality
        public bool Equals(Money? other)
        {
            if (other is null) return false;
            return Amount == other.Amount && Currency == other.Currency;
        }

        public override bool Equals(object? obj) => Equals(obj as Money);
        public override int GetHashCode() => HashCode.Combine(Amount, Currency);
        public override string ToString() => $"{Amount:F2} {Currency}";
    }

    // ────────────────────────────────────────────────────────────────────────
    // VALUE OBJECT 3: ProductImages
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// ✅ PRODUCT IMAGES VALUE OBJECT
    /// Benefits:
    /// - Encapsulates collection of image URLs
    /// - No arbitrary limit (ImageUrl1, ImageUrl2, etc.)
    /// - Validation for each URL centralized
    /// - Ordering maintained
    /// </summary>
    public sealed class ProductImages : IEquatable<ProductImages>
    {
        private static readonly Regex UrlPattern =
            new Regex(@"^https?://", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public IReadOnlyList<string> Urls { get; }

        private ProductImages(IReadOnlyList<string> urls)
        {
            Urls = urls;
        }

        // ✅ Factory with validation
        public static ProductImages? Create(IEnumerable<string> urls)
        {
            if (urls == null) return null;

            var urlList = urls
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .Select(url => url.Trim())
                .ToList();

            if (!urlList.Any()) return null;

            // ✅ Validate all URLs
            if (urlList.Any(url => !UrlPattern.IsMatch(url)))
                return null;

            return new ProductImages(urlList.AsReadOnly());
        }

        // ✅ Create with single URL
        public static ProductImages? CreateSingle(string url)
        {
            return Create(new[] { url });
        }

        // ✅ Add URL (returns new instance - immutable!)
        public ProductImages AddUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL cannot be empty");

            if (!UrlPattern.IsMatch(url))
                throw new ArgumentException("Invalid URL format");

            var newUrls = Urls.ToList();
            newUrls.Add(url);

            return new ProductImages(newUrls.AsReadOnly());
        }

        // ✅ Query methods
        public string PrimaryImage => Urls.First();
        public int Count => Urls.Count;

        // ✅ Value equality
        public bool Equals(ProductImages? other)
        {
            if (other is null) return false;
            return Urls.SequenceEqual(other.Urls);
        }

        public override bool Equals(object? obj) => Equals(obj as ProductImages);
        public override int GetHashCode() => Urls.GetHashCode();
    }

    // ────────────────────────────────────────────────────────────────────────
    // VALUE OBJECT 4: Dimensions
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// ✅ DIMENSIONS VALUE OBJECT
    /// Benefits:
    /// - All dimension data together
    /// - Validation in one place
    /// - Unit conversion built-in
    /// - Can't have inconsistent dimensions (L=10cm, W=20in)
    /// </summary>
    public sealed class Dimensions : IEquatable<Dimensions>
    {
        private static readonly HashSet<string> ValidUnits =
            new() { "CM", "IN", "MM", "M" };

        public decimal Length { get; }
        public decimal Width { get; }
        public decimal Height { get; }
        public string Unit { get; }

        private Dimensions(decimal length, decimal width, decimal height, string unit)
        {
            Length = length;
            Width = width;
            Height = height;
            Unit = unit;
        }

        // ✅ Factory with validation
        public static Dimensions? Create(
            decimal length,
            decimal width,
            decimal height,
            string unit)
        {
            // ✅ Validate all dimensions together
            if (length <= 0 || width <= 0 || height <= 0) return null;

            var normalizedUnit = unit.ToUpperInvariant();
            if (!ValidUnits.Contains(normalizedUnit)) return null;

            return new Dimensions(length, width, height, normalizedUnit);
        }

        // ✅ Domain calculation
        public decimal Volume => Length * Width * Height;

        // ✅ Unit conversion
        public Dimensions ConvertTo(string targetUnit)
        {
            var normalizedTarget = targetUnit.ToUpperInvariant();
            if (!ValidUnits.Contains(normalizedTarget))
                throw new ArgumentException("Invalid unit");

            if (Unit == normalizedTarget)
                return this;

            var conversionFactor = GetConversionFactor(Unit, normalizedTarget);

            return new Dimensions(
                Length * conversionFactor,
                Width * conversionFactor,
                Height * conversionFactor,
                normalizedTarget
            );
        }

        private static decimal GetConversionFactor(string fromUnit, string toUnit)
        {
            // Simplified conversion (CM as base)
            var toCm = fromUnit switch
            {
                "MM" => 0.1m,
                "CM" => 1m,
                "M" => 100m,
                "IN" => 2.54m,
                _ => 1m
            };

            var fromCm = toUnit switch
            {
                "MM" => 10m,
                "CM" => 1m,
                "M" => 0.01m,
                "IN" => 0.393701m,
                _ => 1m
            };

            return toCm * fromCm;
        }

        // ✅ Value equality
        public bool Equals(Dimensions? other)
        {
            if (other is null) return false;
            return Length == other.Length
                && Width == other.Width
                && Height == other.Height
                && Unit == other.Unit;
        }

        public override bool Equals(object? obj) => Equals(obj as Dimensions);
        public override int GetHashCode() =>
            HashCode.Combine(Length, Width, Height, Unit);

        public override string ToString() =>
            $"{Length} x {Width} x {Height} {Unit}";
    }

    // ╔═══════════════════════════════════════════════════════════════════════╗
    // ║                   PRODUCT WITH VALUE OBJECTS                           ║
    // ╚═══════════════════════════════════════════════════════════════════════╝

    /// <summary>
    /// ✅ PRODUCT USING VALUE OBJECTS
    /// Benefits:
    /// - Type safety (can't swap SKU and Name parameters)
    /// - No validation needed (value objects are always valid)
    /// - Clear intent (Money vs decimal)
    /// - IDE help (different types)
    /// </summary>
    public class Product_ValueObjects
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }

        // ✅ Value objects - not primitives!
        private SKU _sku;
        private Money _price;
        private ProductImages? _images;
        private Dimensions? _dimensions;

        public SKU SKU => _sku;
        public Money Price => _price;
        public ProductImages? Images => _images;
        public Dimensions? Dimensions => _dimensions;

        private Product_ValueObjects() { }

        // ✅ Factory method with value objects
        public static Product_ValueObjects Create(
            string name,
            SKU sku,           // ✅ Can't pass invalid SKU
            Money price,       // ✅ Can't pass negative price
            ProductImages? images = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required");

            // ✅ No validation needed - value objects are already valid!
            ArgumentNullException.ThrowIfNull(sku);
            ArgumentNullException.ThrowIfNull(price);

            return new Product_ValueObjects
            {
                Id = Guid.NewGuid(),
                Name = name,
                _sku = sku,
                _price = price,
                _images = images
            };
        }

        // ✅ Business methods with value objects
        public void ChangePrice(Money newPrice)
        {
            ArgumentNullException.ThrowIfNull(newPrice);
            // ✅ No validation - Money is always valid!
            _price = newPrice;
        }

        public void SetImages(ProductImages images)
        {
            ArgumentNullException.ThrowIfNull(images);
            // ✅ No validation - ProductImages are already validated!
            _images = images;
        }

        public void SetDimensions(Dimensions dimensions)
        {
            ArgumentNullException.ThrowIfNull(dimensions);
            // ✅ No validation - Dimensions are already validated!
            _dimensions = dimensions;
        }

        // ✅ Domain calculations using value object methods
        public Money CalculateShippingCost()
        {
            if (_dimensions == null)
                return Money.Create(10m, "USD")!; // Flat rate

            var volume = _dimensions.Volume;
            var cost = volume * 0.01m; // Example calculation

            return Money.Create(cost, "USD")!;
        }
    }

    // ╔═══════════════════════════════════════════════════════════════════════╗
    // ║              SERVICE LAYER WITH VALUE OBJECTS                          ║
    // ╚═══════════════════════════════════════════════════════════════════════╝

    /// <summary>
    /// ✅ CLEAN SERVICE WITH VALUE OBJECTS
    /// Benefits:
    /// - No scattered validation
    /// - Value objects handle their own validation
    /// - Type safety prevents errors
    /// - Clean, readable code
    /// </summary>
    public class ProductService_ValueObjects
    {
        public Product_ValueObjects CreateProduct(
            string name,
            string skuValue,
            decimal priceAmount,
            string currency,
            List<string> imageUrls)
        {
            // ✅ Create value objects (validation happens here)
            var sku = SKU.Create(skuValue);
            if (sku == null)
                throw new ArgumentException("Invalid SKU format");

            var price = Money.Create(priceAmount, currency);
            if (price == null)
                throw new ArgumentException("Invalid price");

            var images = ProductImages.Create(imageUrls);
            // images can be null - it's optional

            // ✅ Create product - no validation needed!
            return Product_ValueObjects.Create(name, sku, price, images);
        }

        public void UpdatePrice(Product_ValueObjects product, decimal amount, string currency)
        {
            // ✅ Create Money value object
            var newPrice = Money.Create(amount, currency);
            if (newPrice == null)
                throw new ArgumentException("Invalid price");

            // ✅ Use domain method
            product.ChangePrice(newPrice);

            // ✅ No validation duplication!
        }

        public void AddImage(Product_ValueObjects product, string url)
        {
            // ✅ Value object handles validation
            var currentImages = product.Images;

            if (currentImages == null)
            {
                var newImages = ProductImages.CreateSingle(url);
                if (newImages != null)
                    product.SetImages(newImages);
            }
            else
            {
                var updatedImages = currentImages.AddUrl(url);
                product.SetImages(updatedImages);
            }
        }

        public void SetDimensions(
            Product_ValueObjects product,
            decimal length,
            decimal width,
            decimal height,
            string unit)
        {
            // ✅ Create Dimensions value object
            var dimensions = Dimensions.Create(length, width, height, unit);
            if (dimensions == null)
                throw new ArgumentException("Invalid dimensions");

            product.SetDimensions(dimensions);

            // ✅ All validation in Dimensions.Create!
        }
    }

    // ╔═══════════════════════════════════════════════════════════════════════╗
    // ║                        COMPARISON SUMMARY                              ║
    // ╚═══════════════════════════════════════════════════════════════════════╝

    /*
     * COMPARISON:
     *
     * ┌──────────────────────┬─────────────────────┬─────────────────────┐
     * │ Aspect               │ Primitives ❌       │ Value Objects ✅     │
     * ├──────────────────────┼─────────────────────┼─────────────────────┤
     * │ Domain Meaning       │ None (just string)  │ Clear (SKU, Money)  │
     * │ Validation           │ Scattered, repeated │ Centralized, once   │
     * │ Type Safety          │ Weak                │ Strong              │
     * │ Invalid Values       │ Possible            │ Impossible          │
     * │ Operations           │ Manual              │ Built-in            │
     * │ Consistency          │ Hard                │ Guaranteed          │
     * │ Testability          │ Hard                │ Easy                │
     * │ IDE Support          │ Poor                │ Excellent           │
     * └──────────────────────┴─────────────────────┴─────────────────────┘
     *
     * WHEN TO CREATE VALUE OBJECT:
     *
     * Create a value object when:
     * ✅ The concept has validation rules (SKU format, Money non-negative)
     * ✅ Multiple primitives belong together (amount + currency)
     * ✅ The concept has domain operations (Money.Add, Dimensions.Volume)
     * ✅ You find yourself repeating validation
     * ✅ Two parameters of same type could be swapped by mistake
     *
     * DON'T create value object when:
     * ❌ It's truly just a primitive with no rules (e.g., "notes" field)
     * ❌ No validation needed
     * ❌ No domain meaning
     *
     * HOW TO REFACTOR:
     *
     * 1. Identify primitive that needs validation/has domain meaning
     * 2. Create value object class with private constructor
     * 3. Add factory method with validation
     * 4. Make immutable (no setters)
     * 5. Implement value equality
     * 6. Replace primitive with value object in entities
     * 7. Update all usages
     * 8. Remove scattered validation
     */
}
