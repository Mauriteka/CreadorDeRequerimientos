using CreadorDeRequerimientos.Domain.Workspace;

namespace CreadorDeRequerimientos.AppCore.Workspace.Templates;

public sealed class DefaultSurveyTemplateFactory
{
    public IReadOnlyList<SurveyTemplate> CreateDefaultSystemTemplates()
    {
        return
        [
            CreateDefaultSystemTemplate(),
            CreateOperationalProcessTemplate(),
            CreateExistingSystemTemplate(),
            CreateReportingDashboardTemplate()
        ];
    }

    public SurveyTemplate CreateDefaultSystemTemplate()
    {
        return SurveyTemplate.Create(
            "Levantamiento base de requerimientos",
            "Plantilla inicial con preguntas frecuentes para entrevistas de descubrimiento.",
            "system",
            [
                new TemplateInterviewSection
                {
                    Title = "Contexto del negocio",
                    Prompt = "Entender el contexto general, el objetivo del area y el problema que se quiere resolver.",
                    Questions =
                    [
                        "Cual es el objetivo del area o proceso?",
                        "Que problema actual quieres resolver?",
                        "Que impacto tiene hoy este problema?",
                        "Que necesidad esperada buscas cubrir con este cambio?"
                    ]
                },
                new TemplateInterviewSection
                {
                    Title = "Usuarios y flujo actual",
                    Prompt = "Descubrir quienes participan y como trabajan hoy en el proceso actual.",
                    Questions =
                    [
                        "Quienes participan en el proceso?",
                        "Como es el flujo actual paso a paso?",
                        "Que herramientas usan actualmente?",
                        "En que parte del flujo aparecen mas fricciones o retrabajos?"
                    ]
                },
                new TemplateInterviewSection
                {
                    Title = "Reglas, datos y excepciones",
                    Prompt = "Capturar reglas de negocio, entradas, salidas, validaciones y escenarios no felices.",
                    Questions =
                    [
                        "Que reglas de negocio no se pueden romper?",
                        "Que datos entran y que datos salen?",
                        "Que errores o excepciones son comunes?",
                        "Hay integraciones, reportes o dependencias tecnicas que debamos considerar?"
                    ]
                },
                new TemplateInterviewSection
                {
                    Title = "Prioridad y criterios de aceptacion",
                    Prompt = "Aterrizar valor esperado, prioridad y forma de validar la solucion.",
                    Questions =
                    [
                        "Como sabremos que esto quedo bien?",
                        "Que prioridad tiene frente a otras necesidades?",
                        "Que criterios de aceptacion deberia cumplir?",
                        "Que riesgos, dependencias o dudas existen?"
                    ]
                }
            ],
            [
                new TemplateMinuteSection { Title = "Problema actual", Prompt = "Resume el problema y el impacto que genera." },
                new TemplateMinuteSection { Title = "Objetivo esperado", Prompt = "Describe el resultado esperado y el valor del cambio." },
                new TemplateMinuteSection { Title = "Usuarios involucrados", Prompt = "Describe los usuarios o areas afectadas." },
                new TemplateMinuteSection { Title = "Flujo actual", Prompt = "Resume el flujo actual y los puntos de friccion detectados." },
                new TemplateMinuteSection { Title = "Reglas de negocio", Prompt = "Lista las reglas, restricciones y validaciones clave." },
                new TemplateMinuteSection { Title = "Datos e integraciones", Prompt = "Documenta entradas, salidas e integraciones necesarias." },
                new TemplateMinuteSection { Title = "Criterios de aceptacion", Prompt = "Escribe criterios observables para validar la solucion." },
                new TemplateMinuteSection { Title = "Prioridad", Prompt = "Indica la prioridad relativa y el motivo." },
                new TemplateMinuteSection { Title = "Riesgos y dependencias", Prompt = "Anota bloqueos, dependencias y dudas abiertas." }
            ]);
    }

    private static SurveyTemplate CreateOperationalProcessTemplate()
    {
        return SurveyTemplate.Create(
            "Proceso operativo de area",
            "Para entender un proceso actual, sus roles, entradas, salidas y puntos de control.",
            "system",
            [
                new TemplateInterviewSection
                {
                    Title = "Inicio y objetivo",
                    Prompt = "Ubicar el disparador del proceso y el resultado esperado.",
                    Questions =
                    [
                        "Que evento inicia este proceso?",
                        "Cual es el resultado esperado al terminar?",
                        "Con que frecuencia ocurre?",
                        "Que volumen aproximado manejan?"
                    ]
                },
                new TemplateInterviewSection
                {
                    Title = "Roles y responsabilidades",
                    Prompt = "Identificar personas, areas y decisiones dentro del flujo.",
                    Questions =
                    [
                        "Que roles participan y que hace cada uno?",
                        "Quien aprueba o valida cada paso importante?",
                        "Donde hay dependencias con otras areas?",
                        "Que sucede cuando una persona clave no esta disponible?"
                    ]
                },
                new TemplateInterviewSection
                {
                    Title = "Flujo y excepciones",
                    Prompt = "Describir el paso a paso y los caminos alternos.",
                    Questions =
                    [
                        "Cuales son los pasos del flujo actual?",
                        "Que pasos se hacen fuera del sistema?",
                        "Que excepciones son mas comunes?",
                        "Donde se generan esperas, retrabajos o errores?"
                    ]
                }
            ],
            [
                new TemplateMinuteSection { Title = "Disparador del proceso", Prompt = "Explica que inicia el proceso y bajo que condiciones." },
                new TemplateMinuteSection { Title = "Flujo actual", Prompt = "Resume el paso a paso con responsables." },
                new TemplateMinuteSection { Title = "Entradas y salidas", Prompt = "Lista informacion recibida, documentos, datos y resultados." },
                new TemplateMinuteSection { Title = "Controles y aprobaciones", Prompt = "Registra validaciones, autorizaciones y puntos de control." },
                new TemplateMinuteSection { Title = "Dolores detectados", Prompt = "Describe esperas, retrabajos, errores y fricciones." },
                new TemplateMinuteSection { Title = "Oportunidades", Prompt = "Anota mejoras propuestas o automatizaciones candidatas." }
            ]);
    }

    private static SurveyTemplate CreateExistingSystemTemplate()
    {
        return SurveyTemplate.Create(
            "Mejora de sistema existente",
            "Para levantar cambios sobre una pantalla, modulo o flujo que ya esta en uso.",
            "system",
            [
                new TemplateInterviewSection
                {
                    Title = "Uso actual",
                    Prompt = "Entender como se usa hoy la funcionalidad existente.",
                    Questions =
                    [
                        "Que pantalla o modulo usan actualmente?",
                        "Para que tarea concreta lo usan?",
                        "Que funciona bien hoy y no deberia cambiar?",
                        "Que datos o acciones son indispensables?"
                    ]
                },
                new TemplateInterviewSection
                {
                    Title = "Cambio requerido",
                    Prompt = "Aterrizar la mejora esperada y su prioridad.",
                    Questions =
                    [
                        "Que cambio necesitas exactamente?",
                        "Que problema resuelve este cambio?",
                        "Que usuarios se benefician o se ven afectados?",
                        "Que pasaria si este cambio no se hace?"
                    ]
                },
                new TemplateInterviewSection
                {
                    Title = "Validacion",
                    Prompt = "Definir criterios claros para aceptar el cambio.",
                    Questions =
                    [
                        "Como deberia verse o comportarse al terminar?",
                        "Que casos de prueba son obligatorios?",
                        "Que permisos, restricciones o validaciones aplican?",
                        "Hay datos historicos o compatibilidad que cuidar?"
                    ]
                }
            ],
            [
                new TemplateMinuteSection { Title = "Funcionalidad actual", Prompt = "Describe la pantalla, modulo o flujo existente." },
                new TemplateMinuteSection { Title = "Problema del cambio", Prompt = "Resume el dolor o limitacion actual." },
                new TemplateMinuteSection { Title = "Cambio solicitado", Prompt = "Describe la mejora requerida en terminos funcionales." },
                new TemplateMinuteSection { Title = "Impacto", Prompt = "Indica usuarios afectados, beneficios y riesgos." },
                new TemplateMinuteSection { Title = "Criterios de aceptacion", Prompt = "Lista condiciones observables para aceptar la mejora." },
                new TemplateMinuteSection { Title = "Compatibilidad", Prompt = "Anota datos, permisos o comportamientos que deben conservarse." }
            ]);
    }

    private static SurveyTemplate CreateReportingDashboardTemplate()
    {
        return SurveyTemplate.Create(
            "Reporte o dashboard",
            "Para levantar requerimientos de reportes, indicadores, tableros y exportaciones.",
            "system",
            [
                new TemplateInterviewSection
                {
                    Title = "Objetivo de analisis",
                    Prompt = "Entender la decision que el reporte debe facilitar.",
                    Questions =
                    [
                        "Que decision o seguimiento debe apoyar este reporte?",
                        "Quien lo consulta y con que frecuencia?",
                        "Que preguntas debe responder?",
                        "Que formato usan hoy para obtener esta informacion?"
                    ]
                },
                new TemplateInterviewSection
                {
                    Title = "Indicadores y filtros",
                    Prompt = "Definir datos visibles, agrupaciones y filtros.",
                    Questions =
                    [
                        "Que indicadores o columnas son indispensables?",
                        "Por que filtros necesitan consultar la informacion?",
                        "Como se deben agrupar o desglosar los datos?",
                        "Hay metas, semaforos o alertas que mostrar?"
                    ]
                },
                new TemplateInterviewSection
                {
                    Title = "Origen y salida",
                    Prompt = "Aclarar fuentes de datos, frecuencia y exportaciones.",
                    Questions =
                    [
                        "De donde salen los datos?",
                        "Cada cuanto debe actualizarse la informacion?",
                        "Necesitan exportar a Excel, PDF o enviar por correo?",
                        "Quien puede ver la informacion completa o parcial?"
                    ]
                }
            ],
            [
                new TemplateMinuteSection { Title = "Objetivo del reporte", Prompt = "Resume la decision o seguimiento que habilita." },
                new TemplateMinuteSection { Title = "Usuarios y frecuencia", Prompt = "Indica quien lo usa y cada cuando." },
                new TemplateMinuteSection { Title = "Indicadores", Prompt = "Lista metricas, columnas y calculos necesarios." },
                new TemplateMinuteSection { Title = "Filtros y agrupaciones", Prompt = "Documenta formas de consultar y desglosar la informacion." },
                new TemplateMinuteSection { Title = "Fuentes de datos", Prompt = "Anota sistemas, tablas, archivos o capturas fuente." },
                new TemplateMinuteSection { Title = "Exportacion y permisos", Prompt = "Describe salidas, restricciones y seguridad." }
            ]);
    }
}
