# Ubiquitous Language - Catalog Management

## Table of Contents
- [What is Ubiquitous Language?](#what-is-ubiquitous-language)
- [Why It Matters](#why-it-matters)
- [Our Domain Language](#our-domain-language)
- [Terms Dictionary](#terms-dictionary)
- [Code Mapping](#code-mapping)
- [Conversation Examples](#conversation-examples)
- [Terms to Avoid](#terms-to-avoid)
- [Evolution of Language](#evolution-of-language)

---

## What is Ubiquitous Language?

**Ubiquitous Language** is a shared vocabulary used by **both developers and domain experts** when discussing the domain.

### Key Principles

1. **Same words in code and conversation**
   - If business calls it "SKU", code calls it `SKU` (not `ProductCode` or `Identifier`)
   - If business says "Publish", code has `product.Publish()` (not `product.MakeActive()`)

2. **No translation layer**
   ```csharp
   // ❌ WRONG: Translation needed
   // Business says: "Set the compare-at price"
   // Developer writes: product.SetOriginalPrice(price); // Confusion!

   // ✅ CORRECT: Same language
   // Business says: "Set the compare-at price"
   // Developer writes: product.SetCompareAtPrice(price); // Clarity!
   ```

3. **Shapes the model**
   - Language discoveries lead to code changes
   - Code insights lead to language refinements
   - Continuous feedback loop

---

## Why It Matters

### Without Ubiquitous Language

**Conversation:**
> Business Expert: "We need to mark products as out of stock when inventory reaches zero."
>
> Developer: "So you want to set the `is_available` flag to false?"
>
> Business Expert: "No, 'out of stock' doesn't mean 'not available'. A product can be available for backorder even when out of stock."
>
> Developer: "So I need a `backorder_allowed` field?"
>
> Business Expert: "What's a field?"

**Result:** Miscommunication, bugs, wrong features

---

### With Ubiquitous Language

**Conversation:**
> Business Expert: "A Product becomes OutOfStock when StockQuantity reaches zero."
>
> Developer: "Should Product.IsOutOfStock() return true only when StockQuantity is zero, or also when Status is Discontinued?"
>
> Business Expert: "Good point! OutOfStock means inventory is zero BUT the product is still Active. Discontinued products aren't 'out of stock', they're just Discontinued."
>
> Developer: "Got it. So `IsOutOfStock()` checks both StockQuantity AND Status."

**Code:**
```csharp
public bool IsOutOfStock() => StockQuantity == 0 && Status == ProductStatus.Active;
```

**Result:** Precise implementation, no ambiguity

---

## Our Domain Language

### Core Concepts

These terms are used **identically** in code and business conversations:

1. **Product** - An item that can be sold (not "Item", not "SKU", not "Record")
2. **Category** - A grouping of related products (not "Group", not "Tag")
3. **SKU** - Stock Keeping Unit, unique identifier for products
4. **Price** - Current selling price of a product
5. **Compare-At Price** - Original price before discount (not "Original Price", not "MSRP")
6. **Publish** - Make a product visible and purchasable (not "Activate", not "Enable")
7. **Draft** - Product exists but isn't visible to customers yet
8. **Discontinued** - Product is no longer sold (not "Deleted", not "Archived")
9. **Stock Quantity** - Number of units available for sale
10. **Low Stock Threshold** - Quantity below which product is considered "running low"

---

## Terms Dictionary

### Product Lifecycle

#### **Draft**
**Business Definition:** A product that exists in the system but is not yet visible to customers.

**Technical Implementation:**
```csharp
public enum ProductStatus
{
    Draft = 0,      // Created but not published
    Active = 1,     // Visible and purchasable
    Inactive = 2,   // Temporarily hidden
    Discontinued = 3 // No longer sold
}
```

**Usage in Conversation:**
- ✅ "The product is in Draft status"
- ✅ "We have 15 draft products ready to publish"
- ❌ "The product is unpublished" (incorrect term)
- ❌ "The product is in staging" (technical jargon)

**When Used:**
- New products before final review
- Products missing required information (images, descriptions)
- Seasonal products created in advance

---

#### **Publish**
**Business Definition:** Making a draft product visible and available for purchase.

**Technical Implementation:**
```csharp
public void Publish(string updatedBy)
{
    if (_images == null || !_images.Urls.Any())
        throw new InvalidOperationException("Cannot publish product without images");

    Status = ProductStatus.Active;
    UpdateAudit(updatedBy);
    AddDomainEvent(new ProductPublishedEvent(Id, Name));
}
```

**Usage in Conversation:**
- ✅ "Publish the product to make it live"
- ✅ "When was this product published?"
- ❌ "Activate the product" (technical term)
- ❌ "Make the product visible" (too vague)

**Business Rules:**
- Product must have at least one image
- Product must have a valid SKU
- Product must have a price set

---

#### **Discontinue**
**Business Definition:** Stop selling a product permanently (but keep historical data).

**Technical Implementation:**
```csharp
public void Discontinue(string updatedBy)
{
    Status = ProductStatus.Discontinued;
    UpdateAudit(updatedBy);
    AddDomainEvent(new ProductDiscontinuedEvent(Id));
}
```

**Usage in Conversation:**
- ✅ "Discontinue products that are no longer manufactured"
- ✅ "This product has been discontinued"
- ❌ "Delete the product" (delete = permanent removal)
- ❌ "Archive the product" (archive implies storage, not business decision)

**Important Distinction:**
- **Discontinue:** Business decision, product still exists in orders
- **Delete:** Technical operation, removes from database

---

### Pricing Concepts

#### **Price**
**Business Definition:** The current amount customers pay for this product.

**Technical Implementation:**
```csharp
private Money _price;  // Value object

public void ChangePrice(Money newPrice, string updatedBy)
{
    ArgumentNullException.ThrowIfNull(newPrice);

    if (_compareAtPrice != null && !_compareAtPrice.IsGreaterThan(newPrice))
        _compareAtPrice = null; // Clear if no longer valid

    _price = newPrice;
    UpdateAudit(updatedBy);
    AddDomainEvent(new ProductPriceChangedEvent(Id, _price.Amount));
}
```

**Usage in Conversation:**
- ✅ "What's the price of this product?"
- ✅ "Change the price to $99.99"
- ❌ "What's the cost?" (cost = what we pay, price = what customer pays)
- ❌ "Current selling point" (business jargon)

---

#### **Compare-At Price**
**Business Definition:** The original price before a discount, shown to customers to highlight savings.

**Technical Implementation:**
```csharp
private Money? _compareAtPrice;  // Optional

public void SetCompareAtPrice(Money compareAtPrice, string updatedBy)
{
    if (!compareAtPrice.IsGreaterThan(_price))
        throw new InvalidOperationException(
            "Compare-at price must be greater than current price");

    _compareAtPrice = compareAtPrice;
    UpdateAudit(updatedBy);
}
```

**Usage in Conversation:**
- ✅ "Set the compare-at price to $149.99"
- ✅ "The compare-at price shows the original value"
- ❌ "MSRP" (manufacturer term, not our language)
- ❌ "Original price" (too vague)
- ❌ "Was price" (grammatically poor)

**Example Display:**
```
Price: $99.99
Compare At: $149.99
You Save: $50.00 (33% off)
```

**Business Rule:** Compare-At Price must ALWAYS be greater than Price

---

#### **Discount Percentage**
**Business Definition:** The calculated savings shown to customers.

**Technical Implementation:**
```csharp
public decimal? CalculateDiscountPercentage()
{
    if (_compareAtPrice == null) return null;

    var discount = ((_compareAtPrice.Amount - _price.Amount) / _compareAtPrice.Amount) * 100;
    return Math.Round(discount, 2);
}
```

**Usage in Conversation:**
- ✅ "Calculate the discount percentage from compare-at price"
- ✅ "This product has a 25% discount"
- ❌ "Markdown percentage" (retail jargon)

**Note:** This is a CALCULATED value, not stored directly

---

### Inventory Concepts

#### **Stock Quantity**
**Business Definition:** The number of units currently available for sale.

**Technical Implementation:**
```csharp
public int StockQuantity { get; private set; }

public void UpdateStock(int quantity, string updatedBy)
{
    if (quantity < 0)
        throw new ArgumentException("Stock quantity cannot be negative");

    StockQuantity = quantity;
    UpdateAudit(updatedBy);
    AddDomainEvent(new ProductStockChangedEvent(Id, StockQuantity));
}
```

**Usage in Conversation:**
- ✅ "Update the stock quantity to 50 units"
- ✅ "What's the current stock quantity?"
- ❌ "Inventory count" (too vague)
- ❌ "Units on hand" (warehousing term)

---

#### **Out of Stock**
**Business Definition:** A product that is active but has zero units available.

**Technical Implementation:**
```csharp
public bool IsOutOfStock() => StockQuantity == 0 && Status == ProductStatus.Active;
```

**Usage in Conversation:**
- ✅ "This product is out of stock"
- ✅ "Show out-of-stock products"
- ❌ "Unavailable" (could mean discontinued)
- ❌ "Sold out" (implies never restocking)

**Important:** Discontinued products are NOT "out of stock" - they're discontinued!

---

#### **Low Stock**
**Business Definition:** A product with quantity below the low stock threshold.

**Technical Implementation:**
```csharp
public int? LowStockThreshold { get; private set; }

public bool IsLowStock()
{
    if (!LowStockThreshold.HasValue) return false;
    return StockQuantity > 0 && StockQuantity <= LowStockThreshold.Value;
}

public void SetLowStockThreshold(int threshold, string updatedBy)
{
    if (threshold < 0)
        throw new ArgumentException("Threshold cannot be negative");

    LowStockThreshold = threshold;
    UpdateAudit(updatedBy);
}
```

**Usage in Conversation:**
- ✅ "Set low stock threshold to 10 units"
- ✅ "Alert when products are low in stock"
- ❌ "Reorder point" (supply chain term)
- ❌ "Safety stock" (inventory management term)

---

### Product Identification

#### **SKU (Stock Keeping Unit)**
**Business Definition:** A unique code identifying each distinct product.

**Technical Implementation:**
```csharp
public sealed class SKU : ValueObject
{
    private static readonly Regex SkuPattern = new Regex(@"^[A-Z0-9\-_]{3,50}$");

    public string Value { get; }

    public static SKU? Create(string value)
    {
        var normalized = value.Trim().ToUpperInvariant();
        if (!SkuPattern.IsMatch(normalized)) return null;

        return new SKU(normalized);
    }

    public static SKU Generate(string prefix)
    {
        var uniquePart = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        return new SKU($"{prefix}-{uniquePart}");
    }
}
```

**Usage in Conversation:**
- ✅ "What's the SKU for this product?"
- ✅ "Generate a new SKU with prefix SHOE"
- ❌ "Product code" (too generic)
- ❌ "Product ID" (ID = database identifier, SKU = business identifier)

**Format Rules:**
- 3-50 characters
- Uppercase letters, numbers, hyphens, underscores only
- Examples: `SHIRT-001`, `LAPTOP-XPS-13`, `SHOE_NIKE_AIR`

---

#### **Slug**
**Business Definition:** A URL-friendly version of the product name, used in web addresses.

**Technical Implementation:**
```csharp
public sealed class Slug : ValueObject
{
    public string Value { get; }

    public static Slug? Generate(string text)
    {
        var slug = text.ToLowerInvariant();
        slug = RemoveAccents(slug);
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        slug = slug.Trim('-');

        if (string.IsNullOrEmpty(slug)) return null;
        return new Slug(slug);
    }
}
```

**Usage in Conversation:**
- ✅ "Generate a slug from the product name"
- ✅ "The product slug is used in the URL"
- ❌ "URL" (slug is part of URL, not the whole URL)
- ❌ "Permalink" (WordPress term)

**Example:**
```
Product Name: "Nike Air Max 90 - Men's Running Shoes"
Generated Slug: "nike-air-max-90-mens-running-shoes"
Full URL: https://store.com/products/nike-air-max-90-mens-running-shoes
```

---

### Category Concepts

#### **Category**
**Business Definition:** A grouping mechanism for organizing related products.

**Technical Implementation:**
```csharp
public class Category : BaseEntity
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public Guid? ParentCategoryId { get; private set; }
    public bool IsActive { get; private set; }

    public static Category Create(
        Guid tenantId,
        string name,
        string? description,
        Guid? parentCategoryId,
        string createdBy)
    {
        ValidateName(name);

        var category = new Category
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            Description = description,
            ParentCategoryId = parentCategoryId,
            IsActive = true
        };

        category.UpdateAudit(createdBy);
        category.AddDomainEvent(new CategoryCreatedEvent(category.Id, name));

        return category;
    }
}
```

**Usage in Conversation:**
- ✅ "Create a category for electronics"
- ✅ "Which category does this product belong to?"
- ❌ "Collection" (Shopify term, not ours)
- ❌ "Department" (retail term)
- ❌ "Group" (too generic)

---

#### **Parent Category**
**Business Definition:** The category one level above in the hierarchy.

**Technical Implementation:**
```csharp
public Guid? ParentCategoryId { get; private set; }

public void SetParent(Guid? parentId, string updatedBy)
{
    if (parentId.HasValue && parentId.Value == Id)
        throw new InvalidOperationException("Category cannot be its own parent");

    ParentCategoryId = parentId;
    UpdateAudit(updatedBy);
}
```

**Usage in Conversation:**
- ✅ "Set the parent category to 'Electronics'"
- ✅ "This is a child category of 'Clothing'"
- ❌ "Super-category" (not standard terminology)
- ❌ "Category above" (too informal)

**Example Hierarchy:**
```
Electronics (Parent)
└── Laptops (Child)
    └── Gaming Laptops (Grandchild)
```

---

### Product Metadata

#### **Tags**
**Business Definition:** Keywords associated with a product for search and filtering.

**Technical Implementation:**
```csharp
private readonly List<string> _tags = new();
public IReadOnlyCollection<string> Tags => _tags.AsReadOnly();

public void AddTag(string tag, string updatedBy)
{
    ArgumentException.ThrowIfNullOrWhiteSpace(tag);

    var normalized = tag.Trim().ToLowerInvariant();
    if (_tags.Contains(normalized)) return; // Already exists

    _tags.Add(normalized);
    UpdateAudit(updatedBy);
}

public void RemoveTag(string tag, string updatedBy)
{
    var normalized = tag.Trim().ToLowerInvariant();
    if (_tags.Remove(normalized))
        UpdateAudit(updatedBy);
}
```

**Usage in Conversation:**
- ✅ "Add tags to improve product searchability"
- ✅ "Tag this product as 'summer', 'sale', 'featured'"
- ❌ "Labels" (GitHub term)
- ❌ "Keywords" (SEO term, different from tags)

**Examples:** `summer`, `sale`, `new-arrival`, `bestseller`, `eco-friendly`

---

#### **SEO Metadata**
**Business Definition:** Information used by search engines to display and rank the product.

**Technical Implementation:**
```csharp
public sealed class SEOMetadata : ValueObject
{
    public string? MetaTitle { get; }
    public string? MetaDescription { get; }
    public string? MetaKeywords { get; }

    public static SEOMetadata? Create(
        string? metaTitle,
        string? metaDescription,
        string? metaKeywords)
    {
        if (metaTitle?.Length > 60) return null;
        if (metaDescription?.Length > 160) return null;

        return new SEOMetadata(metaTitle, metaDescription, metaKeywords);
    }
}
```

**Usage in Conversation:**
- ✅ "Update SEO metadata for better search rankings"
- ✅ "Set the meta description to appear in Google results"
- ❌ "Search stuff" (too vague)
- ❌ "Google tags" (incorrect)

**Components:**
- **Meta Title:** Appears in search results and browser tab (max 60 chars)
- **Meta Description:** Snippet shown in search results (max 160 chars)
- **Meta Keywords:** Comma-separated keywords (less important now)

---

## Code Mapping

### How Language Maps to Code Elements

| Business Term | Code Element | Type | Location |
|--------------|--------------|------|----------|
| **Product** | `Product` | Entity | `Domain/Entities/Product.cs` |
| **Category** | `Category` | Entity | `Domain/Entities/Category.cs` |
| **SKU** | `SKU` | Value Object | `Domain/ValueObjects/SKU.cs` |
| **Price** | `Money` | Value Object | `Domain/ValueObjects/Money.cs` |
| **Compare-At Price** | `CompareAtPrice` | Property (Money) | `Product.cs:45` |
| **Stock Quantity** | `StockQuantity` | Property (int) | `Product.cs:38` |
| **Publish** | `Publish()` | Method | `Product.cs:156` |
| **Discontinue** | `Discontinue()` | Method | `Product.cs:165` |
| **Draft** | `ProductStatus.Draft` | Enum Value | `Enums/ProductStatus.cs:5` |
| **Out of Stock** | `IsOutOfStock()` | Method | `Product.cs:267` |
| **Low Stock** | `IsLowStock()` | Method | `Product.cs:258` |
| **Tags** | `Tags` | Collection | `Product.cs:52` |
| **Slug** | `Slug` | Value Object | `Domain/ValueObjects/Slug.cs` |
| **SEO Metadata** | `SEOMetadata` | Value Object | `Domain/ValueObjects/SEOMetadata.cs` |

---

### Method Naming Patterns

Our code uses **verb-noun** naming that matches business language:

```csharp
// ✅ Business says "Change the price" → Code has ChangePrice()
public void ChangePrice(Money newPrice, string updatedBy) { }

// ✅ Business says "Update stock" → Code has UpdateStock()
public void UpdateStock(int quantity, string updatedBy) { }

// ✅ Business says "Publish the product" → Code has Publish()
public void Publish(string updatedBy) { }

// ✅ Business says "Add a tag" → Code has AddTag()
public void AddTag(string tag, string updatedBy) { }

// ❌ WRONG: SetPrice() - business doesn't say "set"
// ❌ WRONG: ModifyInventory() - business doesn't say "modify"
// ❌ WRONG: Activate() - business doesn't say "activate"
```

---

## Conversation Examples

### Example 1: Launching New Product

**Using Ubiquitous Language:**

> **Merchant:** "I want to create a new product for summer sandals."
>
> **System:** "Creating a new Product in Draft status..."
>
> **Merchant:** "Set the SKU to SANDAL-SUMMER-2025."
>
> **System:** "SKU set. What's the Price?"
>
> **Merchant:** "Price is $49.99, and set Compare-At Price to $79.99 to show the discount."
>
> **System:** "Price set to $49.99. Compare-At Price set to $79.99. Discount Percentage is 37.5%."
>
> **Merchant:** "Add 100 to Stock Quantity."
>
> **System:** "Stock Quantity updated to 100."
>
> **Merchant:** "Publish the product."
>
> **System:** "Product published successfully and is now Active."

**Code Implementation:**
```csharp
// Create product (Draft status by default)
var sku = SKU.Create("SANDAL-SUMMER-2025")!;
var price = Money.Create(49.99m)!;
var product = Product.Create(tenantId, categoryId, "Summer Sandals", null, sku, price, "merchant");

// Set compare-at price
var compareAt = Money.Create(79.99m)!;
product.SetCompareAtPrice(compareAt, "merchant");

// Update stock
product.UpdateStock(100, "merchant");

// Publish
product.Publish("merchant");

await _unitOfWork.SaveChangesAsync();
```

---

### Example 2: Handling Low Stock

**Using Ubiquitous Language:**

> **Support:** "Customer wants to buy 5 units of SKU LAPTOP-XPS-001, but we're Low Stock."
>
> **Manager:** "What's the current Stock Quantity?"
>
> **Support:** "Stock Quantity is 3."
>
> **Manager:** "Check the Low Stock Threshold."
>
> **Support:** "Low Stock Threshold is 10 units."
>
> **Manager:** "Order more inventory. Meanwhile, customers can still buy the 3 remaining units."

**Code Implementation:**
```csharp
var product = await _unitOfWork.Products.GetBySkuAsync("LAPTOP-XPS-001");

// Check stock status
if (product.IsLowStock())
{
    // Alert purchasing team
    await _notificationService.NotifyLowStockAsync(product.Id);
}

if (product.IsOutOfStock())
{
    // Show "Out of Stock" on website
    return new { Available = false, Message = "Out of Stock" };
}

// Customer can still buy if stock > 0
if (product.StockQuantity >= requestedQuantity)
{
    // Process order...
}
```

---

### Example 3: Seasonal Price Update

**Using Ubiquitous Language:**

> **Marketing:** "We're running a summer sale. Change Price to $39.99 for all products tagged 'summer'."
>
> **Developer:** "Should I update Compare-At Price too?"
>
> **Marketing:** "If the product doesn't have a Compare-At Price, set it to the current Price before changing. If it already has one, keep it."
>
> **Developer:** "Got it. So preserve the original Compare-At Price to show maximum savings."

**Code Implementation:**
```csharp
var summerProducts = await _unitOfWork.Products
    .GetByTagAsync("summer");

foreach (var product in summerProducts)
{
    // Preserve original compare-at price if it exists
    if (product.CompareAtPrice == null)
    {
        // Set current price as compare-at before changing
        product.SetCompareAtPrice(product.Price, "marketing");
    }

    // Change to sale price
    var salePrice = Money.Create(39.99m)!;
    product.ChangePrice(salePrice, "marketing");
}

await _unitOfWork.SaveChangesAsync();
```

---

## Terms to Avoid

### ❌ Technical Jargon in Domain Layer

**Avoid:**
- "Entity" → Use "Product" or "Category"
- "Record" → Use specific domain term
- "Row" → Database term, not domain term
- "Table" → Database term, not domain term
- "Object" → Use specific domain term
- "Data" → Too vague
- "Model" → Use "Product", "Category", etc.

**Example:**
```csharp
// ❌ WRONG: Technical language
public async Task<RecordDto> GetEntityData(int id) { }

// ✅ CORRECT: Domain language
public async Task<ProductDto> GetProductAsync(Guid productId) { }
```

---

### ❌ Vague Business Terms

**Avoid:**
- "Item" → Too generic (use "Product")
- "Thing" → Way too vague
- "Stuff" → Not professional
- "Details" → Be specific (Product details? Price details?)
- "Information" → What kind of information?

**Example:**
```csharp
// ❌ WRONG: Vague terms
public class ItemInfo { }
public void UpdateThingDetails() { }

// ✅ CORRECT: Specific terms
public class ProductDto { }
public void UpdateProductPrice() { }
```

---

### ❌ Abbreviations (Unless Standard)

**Avoid:**
- "Prod" → Use "Product"
- "Cat" → Use "Category"
- "Desc" → Use "Description"
- "Qty" → Use "Quantity"

**Exceptions (Standard Abbreviations):**
- ✅ "SKU" → Stock Keeping Unit (industry standard)
- ✅ "SEO" → Search Engine Optimization (universally known)
- ✅ "URL" → Uniform Resource Locator (standard)

**Example:**
```csharp
// ❌ WRONG: Non-standard abbreviations
public class ProdCat { }
public int Qty { get; set; }

// ✅ CORRECT: Full terms or standard abbreviations
public class ProductCategory { }
public int Quantity { get; set; }
public SKU SKU { get; set; } // OK - industry standard
```

---

### ❌ Implementation Details Leaking

**Avoid exposing how something is done in domain language:**

```csharp
// ❌ WRONG: Implementation details in names
public class ProductDbEntity { }  // "Db" is implementation detail
public void SaveToDatabase() { }   // "Database" is infrastructure
public bool IsInCache { get; }     // "Cache" is technical concern

// ✅ CORRECT: Domain concepts only
public class Product { }
public void Save() { }
public bool IsActive { get; }
```

---

### ❌ Terms from Other Domains

**Don't mix terminology from different e-commerce platforms:**

**Shopify Terms (Don't Use):**
- "Collection" → We use "Category"
- "Variant" → We don't have this (yet)
- "Vendor" → We use "Tenant" or "Merchant"

**Amazon Terms (Don't Use):**
- "ASIN" → We use "SKU"
- "Listing" → We use "Product"

**Magento Terms (Don't Use):**
- "Simple Product" → We use "Product"
- "Configurable Product" → We don't have this

**Example:**
```csharp
// ❌ WRONG: Mixing platform terminology
public class Collection { }  // Shopify term
public string ASIN { get; }  // Amazon term

// ✅ CORRECT: Our ubiquitous language
public class Category { }
public SKU SKU { get; }
```

---

## Evolution of Language

### How Language Changes

Ubiquitous language is **not static**. It evolves through:

1. **Domain Discoveries**
   - New insights from business experts
   - Edge cases that reveal ambiguity
   - Customer feedback

2. **Code Insights**
   - Patterns that emerge during implementation
   - Awkward code that signals language problems
   - Refactorings that clarify concepts

---

### Example: Evolution of "Compare-At Price"

**Original Language (Month 1):**
- Term: "Original Price"
- Problem: Ambiguous - original as in "first ever" or "before discount"?

**Refined Language (Month 2):**
- Term: "Was Price"
- Problem: Grammatically awkward, not professional

**Final Language (Month 3):**
- Term: "Compare-At Price"
- Reasoning: Matches how customers think ("compare at $149.99")
- Industry standard in e-commerce

**Code Evolution:**
```csharp
// Month 1
public decimal? OriginalPrice { get; set; }  // Ambiguous

// Month 2
public decimal? WasPrice { get; set; }  // Awkward

// Month 3
public Money? CompareAtPrice { get; set; }  // Clear!
```

---

### Example: Discovery of "Low Stock Threshold"

**Original Understanding:**
- "Alert when stock is low"
- Problem: What is "low"? 10 units? 5 units? 1 unit?

**Domain Expert Conversation:**
> **Developer:** "When should we alert for low stock?"
>
> **Expert:** "It depends on the product. For popular items, 50 units might be low. For slow-moving items, 5 units is plenty."
>
> **Developer:** "So each product needs its own threshold?"
>
> **Expert:** "Exactly! Let merchants set a Low Stock Threshold for each product."

**Language Added:**
- **New Term:** "Low Stock Threshold"
- **Definition:** Configurable quantity below which product is considered low stock

**Code Added:**
```csharp
public int? LowStockThreshold { get; private set; }

public void SetLowStockThreshold(int threshold, string updatedBy)
{
    if (threshold < 0)
        throw new ArgumentException("Threshold cannot be negative");

    LowStockThreshold = threshold;
    UpdateAudit(updatedBy);
}

public bool IsLowStock()
{
    if (!LowStockThreshold.HasValue) return false;
    return StockQuantity > 0 && StockQuantity <= LowStockThreshold.Value;
}
```

---

## Language Governance

### Rules for Adding New Terms

1. **Verify with Domain Expert**
   - Don't invent terms alone
   - Confirm usage with business stakeholders

2. **Document Immediately**
   - Add to this document
   - Update code comments
   - Communicate to team

3. **Update Everywhere**
   - Code (classes, methods, properties)
   - Tests (test names use domain language)
   - Documentation
   - API endpoints
   - UI labels

4. **No Synonyms**
   - One concept = one term
   - If you have "Product" don't also use "Item"
   - Consistency is critical

---

### Red Flags

Signs your ubiquitous language needs improvement:

1. **Developers translating for business:**
   - "When they say X, we code it as Y"
   - This translation = language gap!

2. **Multiple terms for same concept:**
   - "Product", "Item", "Article" all mean the same thing
   - Pick ONE and use it everywhere

3. **Code doesn't match conversation:**
   - Meeting: "We need to publish products"
   - Code: `product.SetStatus(ProductStatus.Active)`
   - Mismatch! Code should be: `product.Publish()`

4. **Explaining code to business:**
   - If you need to explain what `ResolveInventoryConstraints()` means
   - The method name is wrong - use business language

---

## Summary

### Key Principles

1. **Same Language Everywhere**
   - Conversations, code, documentation, tests, UI

2. **No Translation**
   - Code IS the language, not a translation of it

3. **Business-Driven**
   - Terms come from domain experts, not developers

4. **Evolve Together**
   - Language and model grow together through feedback

5. **Precise and Unambiguous**
   - Each term has exactly one meaning
   - Each concept has exactly one term

---

### Quick Reference

**Product Lifecycle:**
- Draft → Publish → Active
- Active → Discontinue → Discontinued
- Active → Deactivate → Inactive → Activate → Active

**Stock States:**
- In Stock (StockQuantity > LowStockThreshold)
- Low Stock (0 < StockQuantity ≤ LowStockThreshold)
- Out of Stock (StockQuantity = 0 AND Status = Active)

**Pricing:**
- Price (what customer pays)
- Compare-At Price (original price, optional)
- Discount Percentage (calculated from above)

---

### Further Learning

- Read source code in `/src/Domain/` - it's written in ubiquitous language
- Read tests in `/tests/Domain.Tests/` - test names use domain language
- Read `/docs/DDD-PATTERNS-CATALOG.md` for implementation patterns
- Read `/docs/AGGREGATE-DESIGN-DECISIONS.md` for boundary decisions

---

*This document is living and evolves with our domain understanding. Last updated: 2025-11-01*
