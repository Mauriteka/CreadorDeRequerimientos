using CreadorDeRequerimientos.Domain.Workspace;

namespace CreadorDeRequerimientos.AppCore.Workspace.Templates;

public sealed class DefaultSurveyTemplateFactory
{
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
}
