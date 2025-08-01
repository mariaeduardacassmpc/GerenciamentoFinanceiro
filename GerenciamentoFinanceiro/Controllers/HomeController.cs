using GerenciamentoFinanceiro.Data;
using GerenciamentoFinanceiro.Models;
using GerenciamentoFinanceiro.Data;
using GerenciamentoFinanceiro.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

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
            ViewBag.DataOperacao = Filtros.ValoresDataOperacao;

            IQueryable<Financeiro> consulta = _context.Financas
               .Include(x => x.transacao).Include(x => x.Categoria);

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
                    consulta = consulta.Where(c => c.DataDaOperacao < hoje);
                }

                if (filtros.EFuturo)
                {

                    consulta = consulta.Where(c => c.DataDaOperacao > hoje);

                }

                if (filtros.EHoje)
                {

                    consulta = consulta.Where(c => c.DataDaOperacao == hoje);

                }
            }

            var financas = consulta.OrderBy(d => d.DataDaOperacao).ToList();

            return View(financas);
        }


        public IActionResult AdicionarTransacao()
        {

            ViewBag.Categorias = _context.Categorias.ToList();
            ViewBag.Transacoes = _context.Transacoes.ToList();

            return View();
        }

        public IActionResult RemoverTransacao(int id)
        {
            var financa = _context.Financas.Find(id);

            _context.Remove(financa);
            _context.SaveChanges();

            return RedirectToAction("AdicionarCategoria");
        }

        public IActionResult EditarTransacao(int id)
        {
            var transacao = _context.Financas
                .FirstOrDefault(f => f.Id == id);

            if (transacao == null)
                return NotFound();

            ViewBag.Categorias = _context.Categorias.ToList();
            ViewBag.Transacoes = _context.Transacoes.ToList();

            return View(transacao);
        }

        [HttpPost]
        public IActionResult EditarTransacao(Financeiro financeiro)
        {
            if (ModelState.IsValid)
            {
                _context.Update(financeiro);
                _context.SaveChanges();
            }

            ViewBag.Categorias = _context.Categorias.ToList();
            ViewBag.Transacoes = _context.Transacoes.ToList();
            
            return RedirectToAction("AdicionarCategoria");
        }

        public IActionResult AdicionarCategoria()
        {

            var categoria = new Categoria { CategoriaId = "categoria" };
            ViewBag.Categorias = _context.Categorias.ToList();

            return View(categoria);
        }

        public IActionResult EditarCategoria(string id)
        {
            var categoria = _context.Categorias
            .FirstOrDefault(f => f.CategoriaId == id.ToString());

            if (categoria == null)
                return NotFound();

            ViewBag.Categorias = _context.Categorias.ToList();
            ViewBag.Transacoes = _context.Transacoes.ToList();

            return RedirectToAction("AdicionarCategoria");
        }

        [HttpPost]
        public IActionResult EditarCategoria(Categoria categoria)
        {
            if (ModelState.IsValid)
            {
                _context.Update(categoria);
                _context.SaveChanges();
            }

            ViewBag.Categorias = _context.Categorias.ToList();
            return RedirectToAction("AdicionarCategoria" );
        }

        public IActionResult RemoverCategoria(string id)
        {
            var categoria = _context.Categorias.Find(id);

            bool categoriaEmUso = _context.Financas.Any(f => f.CategoriaId == id);
            if (categoriaEmUso)
            {
                TempData["Erro"] = "N�o � poss�vel excluir a categoria pois existem transa��es vinculadas a ela.";
                return RedirectToAction("AdicionarCategoria");
            }

            if (categoria != null)
            {
                _context.Categorias.Remove(categoria);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return NotFound();
        }

        public IActionResult SomatoriaValores()
        {
            var resultados = from g in _context.Financas
                                        .Include(x => x.Categoria)
                                        .Include(x => x.transacao)
                                        .ToList()
                             group g by new { g.CategoriaId } into total
                             select new
                             {
                                 CategoriaNome = total.First().Categoria.Nome,
                                 TransacaoNome = total.First().transacao.Nome,
                                 DataOperacao = total.First().DataDaOperacao,
                                 Total = total.Sum(c => c.Valor)

                             };

            var ganhos = _context.Financas
                            .Include(x => x.Categoria)
                            .Include(x => x.transacao)
                            .Where(x => x.TransacaoId == "ganho")
                            .Sum(x => x.Valor);

            var gastos = _context.Financas
                            .Include(x => x.Categoria)
                            .Include(x => x.transacao)
                            .Where(x => x.TransacaoId == "gasto")
                            .Sum(x => x.Valor);


            var diferenca = ganhos - gastos;
            List<RegistrosFinanceiros> registros = new List<RegistrosFinanceiros>();

            foreach (var resultado in resultados)
            {
                var registro = new RegistrosFinanceiros()
                {
                    CategoriaNome = resultado.CategoriaNome,
                    TransacaoNome = resultado.TransacaoNome,
                    DataOperacao = resultado.DataOperacao.ToString("dd/MM/yyyy"),
                    ValorCategoria = resultado.Total.ToString("F"),
                    Ganhos = ganhos.ToString("F"),
                    Gastos = gastos.ToString("F"),
                    Diferenca = diferenca.ToString("F"),

                };

                registros.Add(registro);
            }

            return View(registros);
        }

        [HttpPost]
        public IActionResult Filtrar(string[] filtro)
        {
            string id = string.Join("-", filtro);
            return RedirectToAction("Index", new { ID = id });
        }

        [HttpPost]
        public IActionResult AdicionarCategoria(Categoria categoria)
        {
            if (ModelState.IsValid)
            {
                var categoriaBanco = new Categoria
                {
                    CategoriaId = categoria.Nome.ToLower(),
                    Nome = categoria.Nome,
                };

                _context.Categorias.Add(categoriaBanco);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }
            else
            {
                return View(categoria);
            }
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
                ViewBag.Categorias = _context.Categorias.ToList();
                ViewBag.Transacoes = _context.Transacoes.ToList();

                return View(financeiro);
            }

        }
    }
}