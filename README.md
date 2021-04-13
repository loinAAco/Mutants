# Mutants
Prueba de código MELI - MutantesAPI

 **1. Tecnologías utilizadas:**

C# - netcore 3.1 WebApi
XUnit
AWS BeanStalk
AWS DynamoDB
Swagger
Visual Studio Community 2019

 **2. Como ejecutar el proyecto:**

Para ejecutar el proyecto se necesita tener una cuenta AWS (En este caso usé la free-tier) y crear una cuenta con permisos para acceder a DynamoDB programaticamente. https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/SettingUp.DynamoWebService.html

Clonar la solución de este repositorio y abrir en Visual Studio, se debe configurar un profile con AWS Toolkit for VisualStudio (Con la cuenta creada anteriormente) y ejecutar el sitio. Localmente se ejecuta en http://localhost:29220/ y se puede acceder a /mutant, /stats y /swagger.

Si se desea ejecutar en el servidor remoto las urls a utilizar son las siguientes: 

http://mutants-dev.us-east-2.elasticbeanstalk.com/mutant/
http://mutants-dev.us-east-2.elasticbeanstalk.com/stats/

 **3. Pruebas unitarias:**

Las pruebas codificadas para la solución sobrepasa el 80% como fue solicitado.

