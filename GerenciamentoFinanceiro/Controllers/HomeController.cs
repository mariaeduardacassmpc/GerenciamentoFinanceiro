using System.Diagnostics;
using GerenciamentoFinanceiro.Data;
using GerenciamentoFinanceiro.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

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
            ViewBag.DataOperacao = filtros.DataOperacao; 

            IQueryable<Financeiro> consulta = _context.Financas
                                                        .Include(x => x.transacao)
                                                        .Include(x => x.Categoria);
            if (filtros.TemCategoria)
            {
                consulta = consulta.Where(c => c.CategoriaId == filtros.CategoriaId);
            }

            if (filtros.TemTransacao)
            {
                consulta = consulta.Where(c => c.TransacaoId == filtros.TransacaoId);
            }

            if (filtros.TemDataOperacao)
            {
                var hoje = DateTime.Today;

                if (filtros.EPassado)
                {
                    consulta = consulta.Where(c => c.DataDaOperacao.Date < hoje);
                }
                else if (filtros.EFuturo)
                {
                    consulta = consulta.Where(c => c.DataDaOperacao.Date > hoje);
                }
                else if (filtros.EHoje)
                {
                    consulta = consulta.Where(c => c.DataDaOperacao.Date == hoje);
                }

                var financas = consulta.OrderBy(d => d.DataDaOperacao).ToList();

                return View(financas);

            }
            return View(new List<Financeiro>());

        }

        [HttpPost]
        public IActionResult AdicionarTransacao(string[] filtro)
        {
            ViewBag.Categoria = _context.Categorias.ToList();
            ViewBag.Transacao = _context.Transacoes.ToList();

            return View();
        }

        [HttpPost]
        public IActionResult Filtrar(string[] filtro)
        {
            string id = string.Join("-", filtro);
            return RedirectToAction("Index", new {ID = id});
        }

        [HttpPost]
        public IActionResult AdicionarTransacao(Financeiro financeiro)
        {
            if (ModelState.IsValid)
            {
                _context.Financas.Add(financeiro);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.Categoria = _context.Categorias.ToList();
                ViewBag.Transacoes = _context.Transacoes.ToList();

                return View(financeiro);
            }
        }

        public IActionResult RemoverTransacao(int id)
        {
            var financa = _context.Financas.Find(id);
            _context.Remove(financa);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
