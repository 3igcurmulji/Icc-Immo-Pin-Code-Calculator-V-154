using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Azure.DataGateway.Service.Tests.CosmosTests
{

    [TestClass, TestCategory(TestCategory.COSMOS)]
    public class QueryFilterTests : TestBase
    {
        private static readonly string _containerName = Guid.NewGuid().ToString();
        private static int _pageSize = 10;
        private static readonly string _graphQLQueryName = "getPlanetsWithFilter";

        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            Init(context);
            Client.CreateDatabaseIfNotExistsAsync(DATABASE_NAME).Wait();
            Client.GetDatabase(DATABASE_NAME).CreateContainerIfNotExistsAsync(_containerName, "/id").Wait();
            CreateItems(DATABASE_NAME, _containerName, 10);
            RegisterGraphQLType("Planet", DATABASE_NAME, _containerName);
            RegisterGraphQLType("PlanetConnection", DATABASE_NAME, _containerName, true);
        }

        /// <summary>
        /// Tests eq of StringFilterInput
        /// </summary>
        [TestMethod]
        public async Task TestStringFiltersEq()
        {
            string gqlQuery = @"{
                getPlanetsWithFilter(first: 10, _filter: {name: {eq: ""Endor""}})
                {
                    items {
                        name
                    }
                }
            }";

            string dbQuery = "select c.name from c where c.name = \"Endor\"";

            await ExecuteAndValidateResult(_graphQLQueryName, gqlQuery, dbQuery);

        }

        private static async Task ExecuteAndValidateResult(string graphQLQueryName, string gqlQuery, string dbQuery)
        {
            JsonElement actual = await ExecuteGraphQLRequestAsync(graphQLQueryName, query: gqlQuery);
            JsonDocument expected = await ExecuteCosmosRequestAsync(dbQuery, _pageSize, null, _containerName);
            ValidateResults(actual.GetProperty("items"), expected.RootElement);
        }

        private static void ValidateResults(JsonElement actual, JsonElement expected)
        {

            Assert.IsNotNull(expected);
            Assert.IsNotNull(actual);
            Assert.IsTrue(JToken.DeepEquals(JToken.Parse(actual.ToString()), JToken.Parse(expected.ToString())));
        }

        /// <summary>
        /// Tests neq of StringFilterInput
        /// </summary>
        [TestMethod]
        public async Task TestStringFiltersNeq()
        {

            string gqlQuery = @"{
                getPlanetsWithFilter(first: 10, _filter: {name: {neq: ""Endor""}})
                {
                    items {
                        name
                    }
                }
            }";

            string dbQuery = "select c.name from c where c.name != \"Endor\"";

            await ExecuteAndValidateResult(_graphQLQueryName, gqlQuery, dbQuery);
        }

        /// <summary>
        /// Tests startsWith of StringFilterInput
        /// </summary>
        [TestMethod]
        public async Task TestStringFiltersStartsWith()
        {
            string gqlQuery = @"{
                getPlanetsWithFilter(first: 10, _filter: {name: {startsWith: ""En""}})
                {
                    items {
                        name
                    }
                }
            }";

            string dbQuery = "select c.name from c where c.name LIKE \"En%\"";

            await ExecuteAndValidateResult(_graphQLQueryName, gqlQuery, dbQuery);
        }

        /// <summary>
        /// Tests endsWith of StringFilterInput
        /// </summary>
        [TestMethod]
        public async Task TestStringFiltersEndsWith()
        {
            string gqlQuery = @"{
                getPlanetsWithFilter(first: 10, _filter: {name: {endsWith: ""h""}})
                {
                    items {
                        name
                    }
                }
            }";

            string dbQuery = "select c.name from c where c.name LIKE \"%h\"";

            await ExecuteAndValidateResult(_graphQLQueryName, gqlQuery, dbQuery);
        }

        /// <summary>
        /// Tests contains of StringFilterInput
        /// </summary>
        [TestMethod]
        public async Task TestStringFiltersContains()
        {
            string gqlQuery = @"{
                getPlanetsWithFilter(first: 10, _filter: {name: {contains: ""pi""}})
                {
                    items {
                        name
                    }
                }
            }";

            string dbQuery = "select c.name from c where c.name LIKE \"%pi%\"";

            await ExecuteAndValidateResult(_graphQLQueryName, gqlQuery, dbQuery);
        }

        /// <summary>
        /// Tests notContains of StringFilterInput
        /// </summary>
        [TestMethod]
        public async Task TestStringFiltersNotContains()
        {
            string gqlQuery = @"{
                getPlanetsWithFilter(first: 10, _filter: {name: {notContains: ""pi""}})
                {
                    items {
                        name
                    }
                }
            }";

            string dbQuery = "select c.name from c where c.name NOT LIKE \"%pi%\"";

            await ExecuteAndValidateResult(_graphQLQueryName, gqlQuery, dbQuery);
        }

        /// <summary>
        /// Tests that special characters are escaped in operations involving LIKE
        /// Special chars not working so ignoring for now!
        /// </summary>
        [Ignore]
        [TestMethod]
        public async Task TestStringFiltersContainsWithSpecialChars()
        {
            string gqlQuery = @"{
                getPlanetsWithFilter(first: 10, _filter: {name: {contains: ""%""}})
                {
                    items {
                        name
                    }
                }
            }";

            string dbQuery = "select c.name from c where c.name LIKE \"%\"";

            await ExecuteAndValidateResult(_graphQLQueryName, gqlQuery, dbQuery);
        }

        /// <summary>
        /// Tests eq of IntFilterInput
        /// </summary>
        [TestMethod]
        public async Task TestIntFiltersEq()
        {
            string gqlQuery = @"{
                getPlanetsWithFilter(first: 10, _filter: {age: {eq: 4}})
                {
                    items {
                        age
                    }
                }
            }";

            string dbQuery = "select c.age from c where c.age = 4";
            await ExecuteAndValidateResult(_graphQLQueryName, gqlQuery, dbQuery);
        }

        /// <summary>
        /// Tests neq of IntFilterInput
        /// </summary>
        [TestMethod]
        public async Task TestIntFiltersNeq()
        {
            string gqlQuery = @"{
                getPlanetsWithFilter(first: 10, _filter: {age: {neq: 4}})
                {
                    items {
                        age
                    }
                }
            }";

            string dbQuery = "select c.age from c where c.age != 4";
            await ExecuteAndValidateResult(_graphQLQueryName, gqlQuery, dbQuery);
        }

        /// <summary>
        /// Tests gt and lt of IntFilterInput
        /// </summary>
        [TestMethod]
        public async Task TestIntFiltersGtLt()
        {
            string gqlQuery = @"{
                getPlanetsWithFilter(first: 10, _filter: {age: {gt: 2 lt: 5}})
                {
                    items {
                        age
                    }
                }
            }";

            string dbQuery = "select c.age from c where c.age > 2 and c.age < 5";
            await ExecuteAndValidateResult(_graphQLQueryName, gqlQuery, dbQuery);
        }

        /// <summary>
        /// Tests gte and lte of IntFilterInput
        /// </summary>
        [TestMethod]
        public async Task TestIntFiltersGteLte()
        {
            string gqlQuery = @"{
                getPlanetsWithFilter(first: 10, _filter: {age: {gte: 2 lte: 5}})
                {
                    items {
                        age
                    }
                }
            }";

            string dbQuery = "select c.age from c where c.age >= 2 and c.age <= 5";
            await ExecuteAndValidateResult(_graphQLQueryName, gqlQuery, dbQuery);
        }

        /// <summary>
        /// Test that:
        /// - the predicate equivalent of *FilterInput input types is put in parenthesis if the
        ///   predicate
        /// - the predicate equivalent of and / or field is put in parenthesis if the predicate
        ///   contains only one operation
        /// </summary>
        /// <remarks>
        /// one operation predicate: id == 2
        /// multiple operation predicate: id == 2 AND publisher_id < 3
        /// </remarks>
        [TestMethod]
        public async Task TestCreatingParenthesis1()
        {
            string gqlQuery = @"{
                getPlanetsWithFilter(first: 10, _filter: {
                                    name: {contains: ""En""}
                                    or: [
                                        {age:{gt: 2 lt: 4}},
                                        {age: {gte: 2}},
                                    ]
                                })
                {
                    items{
                        name
                        age
                    }
                }
            }";

            string dbQuery = "SELECT c.name, c.age FROM c WHERE (c.name LIKE \"En%\" " +
                "AND ((c.age > 2 AND c.age < 4) OR c.age >=2))";

            await ExecuteAndValidateResult(_graphQLQueryName, gqlQuery, dbQuery);
        }

        /// <summary>
        /// Test that:
        /// - the predicate equivalent of *FilterInput input types is put in parenthesis if the
        ///   predicate
        /// - the predicate equivalent of and / or field is put in parenthesis if the predicate
        ///   contains only one operation
        /// </summary>
        /// <remarks>
        /// one operation predicate: id == 2
        /// multiple operation predicate: id == 2 AND publisher_id < 3
        /// </remarks>
        [TestMethod]
        public async Task TestCreatingParenthesis2()
        {
            string gqlQuery = @"{
                getPlanetsWithFilter(first: 10, _filter: {
                                    or: [
                                        {age: {gt: 2} and: [{age: {lt: 4}}]},
                                        {age: {gte: 2} name: {contains: ""En""}}
                                    ]
                                })
                {
                    items{
                        name
                        age
                    }
                }
            }";
            string dbQuery = "SELECT c.name, c.age FROM c WHERE" +
                " ((c.age > 2 AND c.age < 4) OR (c.age >= 2 AND c.name LIKE \"En%\"))";

            await ExecuteAndValidateResult(_graphQLQueryName, gqlQuery, dbQuery);
        }

        /// <summary>
        /// Test that a complicated filter is evaluated as:
        /// - all non and/or fields of each *FilterInput are AND-ed together and put in parenthesis
        /// - each *FilterInput inside an and/or team is AND/OR-ed together and put in parenthesis
        /// - the final predicate is:
        ///   ((<AND-ed non and/or predicates>) AND (<AND-ed predicates in and filed>) OR <OR-ed predicates in or field>)
        /// </summart>
        /// 
        [TestMethod]
        public async Task TestComplicatedFilter()
        {
            string gqlQuery = @"{
                getPlanetsWithFilter(first: 10, _filter: {
                                    age: {gte: 1}
                                    name: {notContains: ""En""}
                                    and: [
                                        {
                                            age: {lt: 6}
                                            name: {startsWith: ""Ma""}
                                        },
                                        {
                                            name: {endsWith: ""s""}
                                            age: {neq: 5}
                                        }
                                    ]
                                    or: [
                                        {dimension: {eq: ""space""}}
                                    ]
                                })
                {
                    items {
                        age
                        name
                        dimension
                    }
                }
            }";

            string dbQuery = "SELECT c.age, c.name, c.dimension FROM c " +
                "WHERE (c.age >= 1 AND c.name NOT LIKE \"%En\" AND" +
                " ((c.age < 6 AND c.name LIKE \"Ma%\") AND (c.name LIKE \"%s\" AND c.age != 5)) AND c.dimension = \"space\")";

            await ExecuteAndValidateResult(_graphQLQueryName, gqlQuery, dbQuery);
        }

        /// <summary>
        /// Test that an empty and evaluates to False
        /// </summary>
        [TestMethod]
        public async Task TestOnlyEmptyAnd()
        {
            string graphQLQueryName = "getPlanetsWithFilter";
            string gqlQuery = @"{
                getPlanetsWithFilter(first: 10, _filter: {and: []})
                {
                    items {
                        id
                    }
                }
            }";

            JsonElement actual = await ExecuteGraphQLRequestAsync(graphQLQueryName, query: gqlQuery);
            Assert.IsTrue(JToken.DeepEquals(actual.GetProperty("items").ToString(), "[]"));
        }

        /// <summary>
        /// Test that an empty or evaluates to False
        /// </summary>
        [TestMethod]
        public async Task TestOnlyEmptyOr()
        {
            string graphQLQueryName = "getPlanetsWithFilter";
            string gqlQuery = @"{
                getPlanetsWithFilter(first: 10, _filter: {or: []})
                {
                    items {
                        id
                    }
                }
            }";

            JsonElement actual = await ExecuteGraphQLRequestAsync(graphQLQueryName, query: gqlQuery);
            Assert.IsTrue(JToken.DeepEquals(actual.GetProperty("items").ToString(), "[]"));
        }

        /// <summary>
        /// Test filtering null integer fields
        /// </summary>
        [TestMethod]
        public async Task TestGetNullIntFields()
        {
            string gqlQuery = @"{
                getPlanetsWithFilter(first: 10, _filter: {age: {isNull: false}})
                {
                    items {
                        name
                        age
                    }
                }
            }";

            string dbQuery = "select c.name, c.age from c where NOT IS_NULL(c.age)";
            await ExecuteAndValidateResult(_graphQLQueryName, gqlQuery, dbQuery);
        }

        /// <summary>
        /// Test filtering non null integer fields
        /// </summary>
        [TestMethod]
        public async Task TestGetNonNullIntFields()
        {
            string gqlQuery = @"{
                getPlanetsWithFilter(first: 10, _filter: {age: {isNull: true}})
                {
                    items {
                        name
                        age
                    }
                }
            }";

            string dbQuery = "select c.name, c.age from c where IS_NULL(c.age)";
            await ExecuteAndValidateResult(_graphQLQueryName, gqlQuery, dbQuery);
        }

        /// <summary>
        /// Test filtering null string fields
        /// </summary>
        [TestMethod]
        public async Task TestGetNullStringFields()
        {
            string gqlQuery = @"{
                getPlanetsWithFilter(first: 10, _filter: {name: {isNull: true}})
                {
                    items {
                        name
                        age
                    }
                }
            }";

            string dbQuery = "select c.name, c.age from c where IS_NULL(c.name)";
            await ExecuteAndValidateResult(_graphQLQueryName, gqlQuery, dbQuery);
        }

        /// <summary>
        /// Test filtering not null string fields
        /// </summary>
        [TestMethod]
        public async Task TestGetNonNullStringFields()
        {
            string gqlQuery = @"{
                getPlanetsWithFilter(first: 10, _filter: {name: {isNull: false}})
                {
                    items {
                        name
                        age
                    }
                }
            }";

            string dbQuery = "select c.name, c.age from c where NOT IS_NULL(c.name)";
            await ExecuteAndValidateResult(_graphQLQueryName, gqlQuery, dbQuery);
        }

        /// <summary>
        /// Passes null to nullable fields and makes sure they are ignored
        /// </summary>
        ///
        [Ignore] //Todo: This test fails on linux/mac due to some string comparisoin issues. 
        [TestMethod]
        public async Task TestExplicitNullFieldsAreIgnored()
        {
            string gqlQuery = @"{
                getPlanetsWithFilter(first: 10, _filter: {age: {gte:2 lte: null}
                                                           name: null
                                                           or: null })
                {
                    items {
                        name
                        age
                    }
                }
            }";

            string dbQuery = "select c.name, c.age from c where c.age >= 2";
            await ExecuteAndValidateResult(_graphQLQueryName, gqlQuery, dbQuery);
        }

        /// <summary>
        /// Passes null to nullable fields and makes sure they are ignored
        /// </summary>
        [TestMethod]
        public async Task TestInputObjectWithOnlyNullFieldsEvaluatesToFalse()
        {
            string gqlQuery = @"{
                getPlanetsWithFilter(first: 10, _filter: {age: {lte: null}})
                {
                    items {
                        name
                        age
                    }
                }
            }";

            string dbQuery = "SELECT c.name, c.age FROM c WHERE 1 != 1";
            await ExecuteAndValidateResult(_graphQLQueryName, gqlQuery, dbQuery);
        }
    }
}