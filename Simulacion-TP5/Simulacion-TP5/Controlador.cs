using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulacion_TP5.Entidades;
using Simulacion_TP5.Entidades.Modelo;
using Simulacion_TP5.Entidades.Randoms;
using Simulacion_TP5.InterfacesUsuario;

namespace Simulacion_TP5
{
    class Controlador
    {
        private Servidor servidor1;
        private Servidor servidor2;
        private Servidor servidor3;
        private Servidor servidor4;
        private Servidor servidor5;
        private Servidor servidorFinalizacion;
        private Pedido pedido;

        // PUNTO 2
        double tiempoPromedioEnsamble = 0;

        // PUNTO 10
        int hora = 0;
        int horaAnterior = 0;
        int contardorEnsamblesPorHora = 0;
        double promedioEnsamblesPorHora = 0;

        public void simular(FormTablero formTablero, int cantidadSimulaciones, int mostrarDesde, int cantidadMostrar, double pedidosPorHora)
        {
            this.servidor1 = new Servidor(1, new GeneradorVAUniforme(new MetodoLenguaje(), 20, 30));
            this.servidor2 = new Servidor(1, new GeneradorVAUniforme(new MetodoLenguaje(), 30, 50));
            this.servidor3 = new Servidor(1, new GeneradorVAExponencialNegativa(new MetodoLenguaje(), 1d/30d));
            this.servidor4 = new Servidor(1, new GeneradorVAUniforme(new MetodoLenguaje(), 10, 20));
            this.servidor5 = new Servidor(2, new GeneradorVAExponencialNegativa(new MetodoLenguaje(), 1d / 5d));
            this.servidorFinalizacion = new Servidor(2, null);
            this.pedido = new Pedido();

            double reloj = 0;
            double relojAnterior = 0;
            int numeroPedidoEvento = 0;
            String evento = Eventos.INICIO;

            // PUNTO 2
            this.tiempoPromedioEnsamble = 0;

            for (int i = 1; i <= cantidadSimulaciones; i++)
            {
                switch (evento)
                {
                    case Eventos.INICIO:
                        numeroPedidoEvento = this.pedido.numeroPedido;
                        pedido.calcularTiempo(reloj);
                        break;

                    case Eventos.LLEGADA_PEDIDO:
                        numeroPedidoEvento = this.pedido.numeroPedido;
                        this.pedido.calcularTiempo(reloj);
                        this.servidor1.tomarPedido(reloj, 0);
                        this.servidor2.tomarPedido(reloj, 0);
                        this.servidor3.tomarPedido(reloj, 0);
                        break;
                    case Eventos.FIN_ACTIVIDAD_1:
                        this.servidor1.finalizarTarea(reloj);
                        numeroPedidoEvento = this.servidor1.numeroPedido;
                        this.servidor4.tomarPedido(reloj, 0);
                        break;
                    case Eventos.FIN_ACTIVIDAD_2:
                        this.servidor2.finalizarTarea(reloj);
                        numeroPedidoEvento = this.servidor2.numeroPedido;
                        this.servidor5.tomarPedido(reloj, 0);
                        break;
                    case Eventos.FIN_ACTIVIDAD_3:
                        this.servidor3.finalizarTarea(reloj);
                        numeroPedidoEvento = this.servidor3.numeroPedido;
                        this.servidorFinalizacion.tomarPedido(reloj, 0);
                        //TODO
                        break;
                    case Eventos.FIN_ACTIVIDAD_4:
                        this.servidor4.finalizarTarea(reloj);
                        numeroPedidoEvento = this.servidor4.numeroPedido;
                        this.servidor5.tomarPedido(reloj, 1);
                        break;
                    case Eventos.FIN_ACTIVIDAD_5:
                        this.servidor5.finalizarTarea(reloj);
                        numeroPedidoEvento = this.servidor5.numeroPedido;
                        this.servidorFinalizacion.tomarPedido(reloj, 1);
                        //TODO
                        break;
                    default:
                        break;
                }

                if (this.servidorFinalizacion.getColas().All(cola => cola.cantidad > 0))
                {
                    this.servidorFinalizacion.finalizarTarea(reloj);
                }

                // PUNTO 2
                if(this.servidorFinalizacion.numeroPedido != 0)
                {
                    this.tiempoPromedioEnsamble = reloj / this.servidorFinalizacion.numeroPedido;
                }

                // PUNTOS 5 Y 6
                this.servidor1.getColas().ForEach(cola => cola.calcularPromedios(reloj, relojAnterior, this.pedido.numeroPedido));
                this.servidor2.getColas().ForEach(cola => cola.calcularPromedios(reloj, relojAnterior, this.pedido.numeroPedido));
                this.servidor3.getColas().ForEach(cola => cola.calcularPromedios(reloj, relojAnterior, this.pedido.numeroPedido));
                this.servidor4.getColas().ForEach(cola => cola.calcularPromedios(reloj, relojAnterior, this.pedido.numeroPedido));
                this.servidor5.getColas().ForEach(cola => cola.calcularPromedios(reloj, relojAnterior, this.pedido.numeroPedido));


                // PUNTO 8
                this.servidor1.calcularPorcentajeOcupacionServidor(reloj, relojAnterior);
                this.servidor2.calcularPorcentajeOcupacionServidor(reloj, relojAnterior);
                this.servidor3.calcularPorcentajeOcupacionServidor(reloj, relojAnterior);
                this.servidor4.calcularPorcentajeOcupacionServidor(reloj, relojAnterior);
                this.servidor5.calcularPorcentajeOcupacionServidor(reloj, relojAnterior);

                // PUNTO 10
                this.horaAnterior = this.hora;
                this.hora = (int)Math.Truncate(reloj / 60);
                if (this.hora != this.horaAnterior)
                {
                    this.contardorEnsamblesPorHora = 0;
                }
                if (this.servidorFinalizacion.numeroPedido != this.servidorFinalizacion.numeroPedidoAnterior)
                {
                    this.contardorEnsamblesPorHora++;
                }
                if (this.servidorFinalizacion.numeroPedido != 0)
                {
                    this.promedioEnsamblesPorHora = (double)this.servidorFinalizacion.numeroPedido / reloj;
                }

                if (i < 10 || i >= cantidadSimulaciones - 10 || (i >= mostrarDesde && i < mostrarDesde + cantidadMostrar))
                {
                    this.agregarFila(formTablero, i, reloj, evento, numeroPedidoEvento);
                }
                if ( i == 10)
                {
                    this.agregarFilaSuspensiva(formTablero);
                }
                if (i == mostrarDesde + cantidadMostrar)
                {
                    this.agregarFilaSuspensiva(formTablero);
                }

                ProximoEvento proximoEvento = this.getProximoEvento();
                evento = proximoEvento.evento;
                relojAnterior = reloj;
                reloj = proximoEvento.reloj;
            }
        }

        private ProximoEvento getProximoEvento()
        {
            double tiempoMinimo = this.pedido.llegada;
            String evento = Eventos.LLEGADA_PEDIDO;
            if (this.servidor1.ocupado && this.servidor1.tiempoFinalizacion < tiempoMinimo)
            {
                tiempoMinimo = this.servidor1.tiempoFinalizacion;
                evento = Eventos.FIN_ACTIVIDAD_1;
            }
            if (this.servidor2.ocupado && this.servidor2.tiempoFinalizacion < tiempoMinimo)
            {
                tiempoMinimo = this.servidor2.tiempoFinalizacion;
                evento = Eventos.FIN_ACTIVIDAD_2;
            }
            if (this.servidor3.ocupado && this.servidor3.tiempoFinalizacion < tiempoMinimo)
            {
                tiempoMinimo = this.servidor3.tiempoFinalizacion;
                evento = Eventos.FIN_ACTIVIDAD_3;
            }
            if (this.servidor4.ocupado && this.servidor4.tiempoFinalizacion < tiempoMinimo)
            {
                tiempoMinimo = this.servidor4.tiempoFinalizacion;
                evento = Eventos.FIN_ACTIVIDAD_4;
            }
            if (this.servidor5.ocupado && this.servidor5.tiempoFinalizacion < tiempoMinimo)
            {
                tiempoMinimo = this.servidor5.tiempoFinalizacion;
                evento = Eventos.FIN_ACTIVIDAD_5;
            }
            return new ProximoEvento(tiempoMinimo, evento);
        }

        private void agregarFila(FormTablero formTablero, int i, double reloj, String evento,
            int numeroPedidoEvento)
        {
            List<String> fila = new List<String>();
            fila.Add(i.ToString());
            fila.Add(reloj.ToString());
            fila.Add(evento);
            fila.Add(numeroPedidoEvento.ToString());

            fila.Add(this.servidorFinalizacion.numeroPedido.ToString());
            fila.Add(this.hora.ToString());
            fila.Add(this.contardorEnsamblesPorHora.ToString());
            fila.Add(this.promedioEnsamblesPorHora.ToString());

            fila.Add(this.servidorFinalizacion.getCola(0).cantidad.ToString());
            fila.Add(this.servidorFinalizacion.getCola(0).cantidadMaxima.ToString());
            fila.Add(this.servidorFinalizacion.getCola(1).cantidad.ToString());
            fila.Add(this.servidorFinalizacion.getCola(1).cantidadMaxima.ToString());

            // PUNTO 2
            fila.Add(this.tiempoPromedioEnsamble.ToString());

            // PUNTO 3
            double proporcionRealizadosSolicitados = 0;
            if (this.pedido.numeroPedido != 0)
            {
                proporcionRealizadosSolicitados = (double)this.servidorFinalizacion.numeroPedido / (double)this.pedido.numeroPedido;
            }
            fila.Add(proporcionRealizadosSolicitados.ToString());

            // PUNTO 7
            double promedioPedidosSistema = 0;
            if (this.servidorFinalizacion.numeroPedido != 0)
            {
                promedioPedidosSistema = this.pedido.numeroPedido / this.servidorFinalizacion.numeroPedido;
            }
            fila.Add(promedioPedidosSistema.ToString());

            fila.Add(this.pedido.tiempo.ToString());
            fila.Add(this.pedido.llegada.ToString());

            fila.Add(this.servidor1.getCola(0).cantidad.ToString());
            fila.Add(this.servidor1.getCola(0).cantidadMaxima.ToString());
            fila.Add(this.servidor1.getCola(0).promedioPermanenciaEnCola.ToString());
            fila.Add(this.servidor1.getCola(0).promedioPedidosEnCola.ToString());
            fila.Add(this.servidor1.getEstado());
            fila.Add(this.servidor1.numeroPedido.ToString());
            fila.Add(this.servidor1.tiempo.ToString());
            fila.Add(this.servidor1.tiempoFinalizacion.ToString());
            fila.Add(this.servidor1.porcentajeTiempoOcupado.ToString());

            fila.Add(this.servidor2.getCola(0).cantidad.ToString());
            fila.Add(this.servidor2.getCola(0).cantidadMaxima.ToString());
            fila.Add(this.servidor2.getCola(0).promedioPermanenciaEnCola.ToString());
            fila.Add(this.servidor2.getCola(0).promedioPedidosEnCola.ToString());
            fila.Add(this.servidor2.getEstado());
            fila.Add(this.servidor2.numeroPedido.ToString());
            fila.Add(this.servidor2.tiempo.ToString());
            fila.Add(this.servidor2.tiempoFinalizacion.ToString());
            fila.Add(this.servidor2.porcentajeTiempoOcupado.ToString());

            fila.Add(this.servidor3.getCola(0).cantidad.ToString());
            fila.Add(this.servidor3.getCola(0).cantidadMaxima.ToString());
            fila.Add(this.servidor3.getCola(0).promedioPermanenciaEnCola.ToString());
            fila.Add(this.servidor3.getCola(0).promedioPedidosEnCola.ToString());
            fila.Add(this.servidor3.getEstado());
            fila.Add(this.servidor3.numeroPedido.ToString());
            fila.Add(this.servidor3.tiempo.ToString());
            fila.Add(this.servidor3.tiempoFinalizacion.ToString());
            fila.Add(this.servidor3.porcentajeTiempoOcupado.ToString());

            fila.Add(this.servidor4.getCola(0).cantidad.ToString());
            fila.Add(this.servidor4.getCola(0).cantidadMaxima.ToString());
            fila.Add(this.servidor4.getCola(0).promedioPermanenciaEnCola.ToString());
            fila.Add(this.servidor4.getCola(0).promedioPedidosEnCola.ToString());
            fila.Add(this.servidor4.getEstado());
            fila.Add(this.servidor4.numeroPedido.ToString());
            fila.Add(this.servidor4.tiempo.ToString());
            fila.Add(this.servidor4.tiempoFinalizacion.ToString());
            fila.Add(this.servidor4.porcentajeTiempoOcupado.ToString());

            fila.Add(this.servidor5.getCola(0).cantidad.ToString());
            fila.Add(this.servidor5.getCola(0).cantidadMaxima.ToString());
            fila.Add(this.servidor5.getCola(0).promedioPermanenciaEnCola.ToString());
            fila.Add(this.servidor5.getCola(0).promedioPedidosEnCola.ToString());
            fila.Add(this.servidor5.getCola(1).cantidad.ToString());
            fila.Add(this.servidor5.getCola(1).cantidadMaxima.ToString());
            fila.Add(this.servidor5.getCola(1).promedioPermanenciaEnCola.ToString());
            fila.Add(this.servidor5.getCola(1).promedioPedidosEnCola.ToString());
            fila.Add(this.servidor5.getEstado());
            fila.Add(this.servidor5.numeroPedido.ToString());
            fila.Add(this.servidor5.tiempo.ToString());
            fila.Add(this.servidor5.tiempoFinalizacion.ToString());
            fila.Add(this.servidor5.porcentajeTiempoOcupado.ToString());

            formTablero.agregarFila(fila);
        }

        private void agregarFilaSuspensiva(FormTablero formTablero)
        {
            formTablero.agregarFilaSuspensiva();
        }
    }
}
