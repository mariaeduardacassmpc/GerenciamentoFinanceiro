using System.Diagnostics;
using GerenciamentoFinanceiro.Data;
using GerenciamentoFinanceiro.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GerenciamentoFinanceiro.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context; 
        }

        public IActionResult Index(string id)
        {
            var filtros = new Filtros(id);

            ViewBag.Filtros = filtros;
            ViewBag.Categorias = _context.Categorias.ToList();
            ViewBag.Transacoes = _context.Transacoes.ToList();

            IQueryable<Financeiro> Consulta = _context.Financas.To

            return View();
        }
    }
}
