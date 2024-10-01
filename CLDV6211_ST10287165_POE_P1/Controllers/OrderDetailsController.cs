/*using CLDV6211_ST10287165_POE_P1.Services;
using CLDV6211_ST10287165_POE_P1.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace CLDV6211_ST10287165_POE_P1.Controllers
{
    public class OrderDetailsController : Controller
    {
        private readonly OrderService _orderService;

        public OrderDetailsController(OrderService orderService)
        {
            _orderService = orderService;
        }

        // GET: OrderDetails
        public async Task<IActionResult> Index(string orderId)
        {
            if (string.IsNullOrEmpty(orderId))
            {
                return NotFound();
            }

            var orderDetails = await _orderService.GetOrderDetailsAsync(orderId);
            return View(orderDetails);
        }

        // GET: OrderDetails/Details/5
        public async Task<IActionResult> Details(string orderId, string rowKey)
        {
            if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(rowKey))
            {
                return NotFound();
            }

            var orderDetail = await _orderService.GetOrderDetailAsync(orderId, rowKey);
            if (orderDetail == null)
            {
                return NotFound();
            }

            return View(orderDetail);
        }

        // GET: OrderDetails/Create
        public IActionResult Create(string orderId)
        {
            if (string.IsNullOrEmpty(orderId))
            {
                return NotFound();
            }

            ViewBag.OrderId = orderId;
            return View();
        }

        // POST: OrderDetails/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderDetail orderDetail)
        {
            if (ModelState.IsValid)
            {
                await _orderService.AddOrderDetailAsync(orderDetail);
                return RedirectToAction(nameof(Index), new { orderId = orderDetail.PartitionKey });
            }

            return View(orderDetail);
        }

        // GET: OrderDetails/Edit/5
        public async Task<IActionResult> Edit(string orderId, string rowKey)
        {
            if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(rowKey))
            {
                return NotFound();
            }

            var orderDetail = await _orderService.GetOrderDetailAsync(orderId, rowKey);
            if (orderDetail == null)
            {
                return NotFound();
            }

            return View(orderDetail);
        }

        // POST: OrderDetails/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string orderId, string rowKey, OrderDetail orderDetail)
        {
            if (orderId != orderDetail.PartitionKey || rowKey != orderDetail.RowKey)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _orderService.UpdateOrderDetailAsync(orderDetail);
                return RedirectToAction(nameof(Index), new { orderId = orderId });
            }

            return View(orderDetail);
        }

        // GET: OrderDetails/Delete/5
        public async Task<IActionResult> Delete(string orderId, string rowKey)
        {
            if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(rowKey))
            {
                return NotFound();
            }

            var orderDetail = await _orderService.GetOrderDetailAsync(orderId, rowKey);
            if (orderDetail == null)
            {
                return NotFound();
            }

            return View(orderDetail);
        }

        // POST: OrderDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string orderId, string rowKey)
        {
            await _orderService.DeleteOrderDetailAsync(orderId, rowKey);
            return RedirectToAction(nameof(Index), new { orderId = orderId });
        }


    }
}
*/