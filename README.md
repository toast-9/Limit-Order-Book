# Limit-Order-Book
Limit Order Book in C#

Limit Order Book (LOB), a data structure commonly used in trading systems to match buy and sell orders based on their prices.

# Overview
The code models an order book where users can place buy and sell orders. It attempts to match these orders based on their prices (highest bid vs. lowest ask). The unmatched orders are stored in the order book for future matching. The code uses object pooling for efficient memory management and provides methods to recycle Order objects.

# Summary
The code is designed with efficiency in mind, using data structures that balance speed and storage needs.
The matching engine uses sorted dictionaries for quick access to the best bid and ask prices, and linked lists for efficient order management at each price level.
Object pooling helps minimize memory usage, making it suitable for environments with high order volume.
The matching logic ensures that orders are filled if possible, and unmatched orders are stored in the book for future matching.
This implementation provides a foundation for a simple but efficient order-matching engine often used in trading applications.
