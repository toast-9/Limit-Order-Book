using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public enum OrderType { Buy, Sell }

public class Order
{
    public int Id { get; set; }
    public OrderType Type { get; set; }
    public double Price { get; set; }
    public int Quantity { get; set; }

    private static Stack<Order> orderPool = new Stack<Order>(); // Object pool for orders

    // Factory method using pooling
    public static Order Create(int id, OrderType type, double price, int quantity)
    {
        Order order = orderPool.Count > 0 ? orderPool.Pop() : new Order();
        order.Id = id;
        order.Type = type;
        order.Price = price;
        order.Quantity = quantity;
        return order;
    }

    // Return the order to the pool instead of letting it be garbage collected
    public void Recycle()
    {
        orderPool.Push(this);
    }
}

public class LimitOrderBook
{
    private SortedDictionary<double, LinkedList<Order>> buyOrders = new SortedDictionary<double, LinkedList<Order>>(Comparer<double>.Create((x, y) => y.CompareTo(x)));
    private SortedDictionary<double, LinkedList<Order>> sellOrders = new SortedDictionary<double, LinkedList<Order>>(Comparer<double>.Default);
    private int nextOrderId = 1;

    public void PlaceOrder(OrderType type, double price, int quantity)
    {
        var newOrder = Order.Create(nextOrderId++, type, price, quantity);

        if (type == OrderType.Buy)
        {
            if (!MatchOrder(newOrder, sellOrders, (buy, sell) => buy.Price >= sell.Price))
                AddOrder(buyOrders, newOrder);
        }
        else
        {
            if (!MatchOrder(newOrder, buyOrders, (sell, buy) => sell.Price <= buy.Price))
                AddOrder(sellOrders, newOrder);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddOrder(SortedDictionary<double, LinkedList<Order>> orders, Order order)
    {
        if (!orders.TryGetValue(order.Price, out LinkedList<Order> orderList))
        {
            orderList = new LinkedList<Order>();
            orders.Add(order.Price, orderList);
        }
        orderList.AddLast(order);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool MatchOrder(Order newOrder, SortedDictionary<double, LinkedList<Order>> oppositeOrders, Func<Order, Order, bool> priceMatch)
    {
        while (newOrder.Quantity > 0 && oppositeOrders.Count > 0)
        {
            var bestPrice = oppositeOrders.Keys.First();
            var bestList = oppositeOrders[bestPrice];
            var oppositeOrder = bestList.First.Value;

            if (!priceMatch(newOrder, oppositeOrder)) break;

            int matchQuantity = Math.Min(newOrder.Quantity, oppositeOrder.Quantity);
            newOrder.Quantity -= matchQuantity;
            oppositeOrder.Quantity -= matchQuantity;

            Console.WriteLine($"Trade Executed: {matchQuantity} units at ${oppositeOrder.Price} between Order {newOrder.Id} and Order {oppositeOrder.Id}");

            if (oppositeOrder.Quantity == 0)
            {
                bestList.RemoveFirst();
                if (bestList.Count == 0) oppositeOrders.Remove(bestPrice);
                oppositeOrder.Recycle(); // Return to pool
            }
            if (newOrder.Quantity == 0)
            {
                newOrder.Recycle(); // Return to pool
                return true;
            }
        }
        return false;
    }

    public void PrintOrderBook()
    {
        Console.WriteLine("Buy Orders:");
        foreach (var entry in buyOrders)
            foreach (var order in entry.Value)
                Console.WriteLine($"ID: {order.Id}, Price: ${order.Price}, Qty: {order.Quantity}");

        Console.WriteLine("Sell Orders:");
        foreach (var entry in sellOrders)
            foreach (var order in entry.Value)
                Console.WriteLine($"ID: {order.Id}, Price: ${order.Price}, Qty: {order.Quantity}");
    }
}

class Program
{
    static void Main()
    {
        LimitOrderBook orderBook = new LimitOrderBook();
        while (true)
        {
            Console.WriteLine("\nEnter a new order (or type 'exit' to finish):");
            Console.Write("Order Type (Buy/Sell): ");
            string typeInput = Console.ReadLine();
            if (typeInput.ToLower() == "exit") break;

            if (!Enum.TryParse(typeInput, true, out OrderType type))
            {
                Console.WriteLine("Invalid order type. Please enter 'Buy' or 'Sell'.");
                continue;
            }

            Console.Write("Price: ");
            if (!double.TryParse(Console.ReadLine(), out double price))
            {
                Console.WriteLine("Invalid price. Please enter a numeric value.");
                continue;
            }

            Console.Write("Quantity: ");
            if (!int.TryParse(Console.ReadLine(), out int quantity))
            {
                Console.WriteLine("Invalid quantity. Please enter an integer value.");
                continue;
            }

            orderBook.PlaceOrder(type, price, quantity);
            orderBook.PrintOrderBook();
        }
    }
}
